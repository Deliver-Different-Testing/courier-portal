namespace CourierPortal.Core.DTOs;

public record QuizDto(int Id, string Title, string? Description, string? Category, int PassMark, bool IsRequired, bool IsActive, DateTime Created, List<QuizQuestionDto>? Questions);
public record CreateQuizDto(string Title, string? Description, string? Category, int PassMark, bool IsRequired, bool IsActive);
public record UpdateQuizDto(string Title, string? Description, string? Category, int PassMark, bool IsRequired, bool IsActive);
public record QuizQuestionDto(int Id, int QuizId, string QuestionText, string QuestionType, int SortOrder, List<QuizOptionDto>? Options);
public record CreateQuizQuestionDto(string QuestionText, string QuestionType, int SortOrder, List<CreateQuizOptionDto>? Options);
public record UpdateQuizQuestionDto(string QuestionText, string QuestionType, int SortOrder);
public record QuizOptionDto(int Id, int QuestionId, string OptionText, bool IsCorrect, int SortOrder);
public record CreateQuizOptionDto(string OptionText, bool IsCorrect, int SortOrder);
public record SubmitQuizAttemptDto(int QuizId, int CourierId, List<SubmitAnswerDto> Answers);
public record SubmitAnswerDto(int QuestionId, int? SelectedOptionId, string? TextAnswer);
public record QuizAttemptDto(int Id, int QuizId, string? QuizTitle, int CourierId, int Score, bool Passed, DateTime StartedAt, DateTime? CompletedAt);
public record QuizAttemptDetailDto(int Id, int QuizId, string? QuizTitle, int CourierId, int Score, bool Passed, DateTime StartedAt, DateTime? CompletedAt, List<QuizAttemptAnswerDto> Answers);
public record QuizAttemptAnswerDto(int Id, int QuestionId, string? QuestionText, int? SelectedOptionId, string? SelectedOptionText, string? TextAnswer);
