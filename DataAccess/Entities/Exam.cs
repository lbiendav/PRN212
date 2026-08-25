namespace ExamManagementSystem.DataAccess.Entities;

/// <summary>
/// Đại diện cho một đề thi trắc nghiệm.
/// </summary>
public class Exam
{
    // Khóa chính tự tăng của đề thi.
    public int Id { get; set; }

    // Tên môn/đề được hiển thị ở trang chủ.
    public string Title { get; set; } = string.Empty;

    // Nhóm môn học, dùng cho chức năng lọc.
    public string Subject { get; set; } = string.Empty;

    // Mô tả ngắn giúp học viên biết nội dung đề.
    public string Description { get; set; } = string.Empty;

    // Thời gian tối đa làm bài, tính bằng phút.
    public int DurationMinutes { get; set; } = 15;

    // Chỉ các đề đang hoạt động mới xuất hiện ở trang chủ học viên.
    public bool IsActive { get; set; } = true;

    // Thời điểm quản trị viên tạo đề.
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Các câu hỏi thuộc đề thi này.
    public ICollection<Question> Questions { get; set; } = new List<Question>();

    // Các lần học viên đã làm đề này.
    public ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
}
