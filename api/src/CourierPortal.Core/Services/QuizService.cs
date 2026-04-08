using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class QuizService : IQuizService
{
    private readonly CourierPortalContext _db;

    public QuizService(CourierPortalContext db) => _db = db;

    public async Task<List<QuizDto>> GetAllQuizzesAsync(bool includeInactive = false)
    {
        var query = _db.Quizzes.Include(q => q.Questions).ThenInclude(q => q.Options).AsQueryable();
        if (!includeInactive) query = query.Where(q => q.IsActive);
        return await query.Select(q => MapQuiz(q)).ToListAsync();
    }

    public async Task<QuizDto?> GetQuizByIdAsync(int id)
    {
        var entity = await _db.Quizzes
            .Include(q => q.Questions.OrderBy(x => x.SortOrder))
            .ThenInclude(q => q.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(q => q.Id == id);
        return entity is null ? null : MapQuiz(entity);
    }

    public async Task<QuizDto> CreateQuizAsync(CreateQuizDto dto)
    {
        var entity = new Quiz
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            PassMark = dto.PassMark,
            IsRequired = dto.IsRequired,
            IsActive = dto.IsActive
        };
        _db.Quizzes.Add(entity);
        await _db.SaveChangesAsync();
        return MapQuiz(entity);
    }

    public async Task<QuizDto?> UpdateQuizAsync(int id, UpdateQuizDto dto)
    {
        var entity = await _db.Quizzes.FindAsync(id);
        if (entity is null) return null;

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Category = dto.Category;
        entity.PassMark = dto.PassMark;
        entity.IsRequired = dto.IsRequired;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return await GetQuizByIdAsync(id);
    }

    public async Task<bool> DeleteQuizAsync(int id)
    {
        var entity = await _db.Quizzes.FindAsync(id);
        if (entity is null) return false;
        _db.Quizzes.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<QuizQuestionDto> AddQuestionAsync(int quizId, CreateQuizQuestionDto dto)
    {
        var question = new QuizQuestion
        {
            QuizId = quizId,
            QuestionText = dto.QuestionText,
            QuestionType = dto.QuestionType,
            SortOrder = dto.SortOrder
        };
        _db.QuizQuestions.Add(question);
        await _db.SaveChangesAsync();

        if (dto.Options?.Any() == true)
        {
            foreach (var opt in dto.Options)
            {
                _db.QuizOptions.Add(new QuizOption
                {
                    QuestionId = question.Id,
                    OptionText = opt.OptionText,
                    IsCorrect = opt.IsCorrect,
                    SortOrder = opt.SortOrder
                });
            }
            await _db.SaveChangesAsync();
        }

        await _db.Entry(question).Collection(q => q.Options).LoadAsync();
        return MapQuestion(question);
    }

    public async Task<QuizQuestionDto?> UpdateQuestionAsync(int questionId, UpdateQuizQuestionDto dto)
    {
        var entity = await _db.QuizQuestions.Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == questionId);
        if (entity is null) return null;

        entity.QuestionText = dto.QuestionText;
        entity.QuestionType = dto.QuestionType;
        entity.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync();
        return MapQuestion(entity);
    }

    public async Task<bool> DeleteQuestionAsync(int questionId)
    {
        var entity = await _db.QuizQuestions.FindAsync(questionId);
        if (entity is null) return false;
        _db.QuizQuestions.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<QuizAttemptDetailDto> SubmitAttemptAsync(SubmitQuizAttemptDto dto)
    {
        var quiz = await _db.Quizzes.Include(q => q.Questions).ThenInclude(q => q.Options).FirstAsync(q => q.Id == dto.QuizId);

        var attempt = new QuizAttempt
        {
            QuizId = dto.QuizId,
            CourierId = dto.CourierId,
            StartedAt = DateTime.UtcNow
        };

        int correctCount = 0;
        int totalGraded = 0;

        foreach (var ans in dto.Answers)
        {
            var question = quiz.Questions.First(q => q.Id == ans.QuestionId);
            var answer = new QuizAttemptAnswer
            {
                QuestionId = ans.QuestionId,
                SelectedOptionId = ans.SelectedOptionId,
                TextAnswer = ans.TextAnswer
            };
            attempt.Answers.Add(answer);

            if (question.QuestionType != "text" && ans.SelectedOptionId.HasValue)
            {
                totalGraded++;
                var option = question.Options.FirstOrDefault(o => o.Id == ans.SelectedOptionId.Value);
                if (option?.IsCorrect == true) correctCount++;
            }
        }

        attempt.Score = totalGraded > 0 ? (int)Math.Round(100.0 * correctCount / totalGraded) : 0;
        attempt.Passed = attempt.Score >= quiz.PassMark;
        attempt.CompletedAt = DateTime.UtcNow;

        _db.QuizAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        return MapAttemptDetail(attempt, quiz);
    }

    public async Task<QuizAttemptDetailDto?> GetAttemptAsync(int attemptId)
    {
        var attempt = await _db.QuizAttempts
            .Include(a => a.Quiz)
            .Include(a => a.Answers).ThenInclude(a => a.Question)
            .Include(a => a.Answers).ThenInclude(a => a.SelectedOption)
            .FirstOrDefaultAsync(a => a.Id == attemptId);
        return attempt is null ? null : MapAttemptDetail(attempt, attempt.Quiz);
    }

    public async Task<List<QuizAttemptDto>> GetAttemptsByCourierAsync(int courierId)
    {
        return await _db.QuizAttempts
            .Include(a => a.Quiz)
            .Where(a => a.CourierId == courierId)
            .OrderByDescending(a => a.StartedAt)
            .Select(a => new QuizAttemptDto(a.Id, a.QuizId, a.Quiz.Title, a.CourierId, a.Score, a.Passed, a.StartedAt, a.CompletedAt))
            .ToListAsync();
    }

    public async Task<List<QuizAttemptDto>> GetAttemptsByQuizAsync(int quizId)
    {
        return await _db.QuizAttempts
            .Include(a => a.Quiz)
            .Where(a => a.QuizId == quizId)
            .OrderByDescending(a => a.StartedAt)
            .Select(a => new QuizAttemptDto(a.Id, a.QuizId, a.Quiz.Title, a.CourierId, a.Score, a.Passed, a.StartedAt, a.CompletedAt))
            .ToListAsync();
    }

    private static QuizDto MapQuiz(Quiz q) => new(
        q.Id, q.Title, q.Description, q.Category, q.PassMark, q.IsRequired, q.IsActive, q.Created,
        q.Questions?.OrderBy(x => x.SortOrder).Select(MapQuestion).ToList()
    );

    private static QuizQuestionDto MapQuestion(QuizQuestion q) => new(
        q.Id, q.QuizId, q.QuestionText, q.QuestionType, q.SortOrder,
        q.Options?.OrderBy(o => o.SortOrder).Select(o => new QuizOptionDto(o.Id, o.QuestionId, o.OptionText, o.IsCorrect, o.SortOrder)).ToList()
    );

    private static QuizAttemptDetailDto MapAttemptDetail(QuizAttempt a, Quiz quiz) => new(
        a.Id, a.QuizId, quiz.Title, a.CourierId, a.Score, a.Passed, a.StartedAt, a.CompletedAt,
        a.Answers.Select(ans => new QuizAttemptAnswerDto(
            ans.Id, ans.QuestionId, ans.Question?.QuestionText, ans.SelectedOptionId, ans.SelectedOption?.OptionText, ans.TextAnswer
        )).ToList()
    );
}
