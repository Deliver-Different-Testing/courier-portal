namespace CourierPortal.Core.Domain.Entities;

public class QuizOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int SortOrder { get; set; }

    public QuizQuestion Question { get; set; } = null!;
}
