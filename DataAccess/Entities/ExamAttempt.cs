namespace ExamManagementSystem.DataAccess.Entities;

/// <summary>
/// Lưu kết quả của một lần học viên nộp bài.
/// </summary>
public class ExamAttempt
{
    // Khóa chính của lần thi.
    public int Id { get; set; }

    // Khóa ngoại người đã làm bài.
    public int UserId { get; set; }

    // Khóa ngoại đề đã làm.
    public int ExamId { get; set; }

    // Số câu trả lời đúng.
    public int CorrectAnswers { get; set; }

    // Tổng số câu tại thời điểm nộp bài.
    public int TotalQuestions { get; set; }

    // Điểm trên thang 10.
    public decimal Score { get; set; }

    // Thời điểm bắt đầu làm bài.
    public DateTime StartedAt { get; set; }

    // Thời điểm nhấn nút nộp bài.
    public DateTime SubmittedAt { get; set; }

    // Người dùng thực hiện lần thi.
    public User User { get; set; } = null!;

    // Đề thi được thực hiện.
    public Exam Exam { get; set; } = null!;

    // Chi tiết lựa chọn của từng câu để phục vụ màn hình xem lại.
    public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
}
