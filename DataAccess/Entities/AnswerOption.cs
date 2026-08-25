namespace ExamManagementSystem.DataAccess.Entities;

/// <summary>
/// Đại diện cho một phương án A/B/C/D của câu hỏi.
/// </summary>
public class AnswerOption
{
    // Khóa chính của phương án.
    public int Id { get; set; }

    // Khóa ngoại trỏ tới câu hỏi.
    public int QuestionId { get; set; }

    // Nội dung phương án trả lời.
    public string Content { get; set; } = string.Empty;

    // Đánh dấu đây có phải đáp án đúng hay không.
    public bool IsCorrect { get; set; }

    // Câu hỏi chứa phương án này.
    public Question Question { get; set; } = null!;
}
