using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

public interface IQuizService
{
    Task<List<QuizDto>> GetAllQuizzesAsync(bool includeInactive = false);
    Task<QuizDto?> GetQuizByIdAsync(int id);
    Task<QuizDto> CreateQuizAsync(CreateQuizDto dto);
    Task<QuizDto?> UpdateQuizAsync(int id, UpdateQuizDto dto);
    Task<bool> DeleteQuizAsync(int id);
    Task<QuizQuestionDto> AddQuestionAsync(int quizId, CreateQuizQuestionDto dto);
    Task<QuizQuestionDto?> UpdateQuestionAsync(int questionId, UpdateQuizQuestionDto dto);
    Task<bool> DeleteQuestionAsync(int questionId);
    Task<QuizAttemptDetailDto> SubmitAttemptAsync(SubmitQuizAttemptDto dto);
    Task<QuizAttemptDetailDto?> GetAttemptAsync(int attemptId);
    Task<List<QuizAttemptDto>> GetAttemptsByCourierAsync(int courierId);
    Task<List<QuizAttemptDto>> GetAttemptsByQuizAsync(int quizId);
}
