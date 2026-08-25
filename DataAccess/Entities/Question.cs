namespace ExamManagementSystem.DataAccess.Entities;

/// <summary>
/// Lưu nội dung một câu hỏi của đề thi.
/// </summary>
public class Question
{
    // Khóa chính của câu hỏi.
    public int Id { get; set; }

    // Khóa ngoại trỏ tới đề thi sở hữu câu hỏi.
    public int ExamId { get; set; }

    // Nội dung câu hỏi hiển thị cho học viên.
    public string Content { get; set; } = string.Empty;

    // Thứ tự câu hỏi trong đề.
    public int OrderNumber { get; set; }

    // Đề thi chứa câu hỏi này.
    public Exam Exam { get; set; } = null!;

    // Danh sách phương án trả lời; một phương án có IsCorrect = true.
    public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
}
