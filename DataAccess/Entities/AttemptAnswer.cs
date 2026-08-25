namespace ExamManagementSystem.DataAccess.Entities;

/// <summary>
/// Lưu phương án người dùng đã chọn cho một câu trong một lần thi.
/// </summary>
public class AttemptAnswer
{
    // Khóa chính của dòng chi tiết.
    public int Id { get; set; }

    // Khóa ngoại trỏ tới lần thi.
    public int ExamAttemptId { get; set; }

    // Câu hỏi đã được trả lời.
    public int QuestionId { get; set; }

    // Phương án đã chọn; có thể null nếu người dùng bỏ trống.
    public int? SelectedOptionId { get; set; }

    // Lưu sẵn kết quả đúng/sai để lịch sử không bị thay đổi về sau.
    public bool IsCorrect { get; set; }

    // Lần thi chứa câu trả lời này.
    public ExamAttempt ExamAttempt { get; set; } = null!;

    // Câu hỏi tương ứng.
    public Question Question { get; set; } = null!;

    // Phương án người dùng đã chọn.
    public AnswerOption? SelectedOption { get; set; }
}
