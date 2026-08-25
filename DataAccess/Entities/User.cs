namespace ExamManagementSystem.DataAccess.Entities;

/// <summary>
/// Đại diện cho một tài khoản đăng nhập trong hệ thống.
/// </summary>
public class User
{
    // Khóa chính tự tăng của bảng Users.
    public int Id { get; set; }

    // Tên đăng nhập duy nhất, dùng khi đăng nhập và tìm kiếm.
    public string Username { get; set; } = string.Empty;

    // Mật khẩu đã được băm; tuyệt đối không lưu mật khẩu gốc.
    public string PasswordHash { get; set; } = string.Empty;

    // Họ tên hiển thị trên giao diện.
    public string FullName { get; set; } = string.Empty;

    // Email duy nhất để quản trị viên tra cứu tài khoản.
    public string Email { get; set; } = string.Empty;

    // Vai trò gồm Student hoặc Admin.
    public string Role { get; set; } = "Student";

    // Cho phép khóa tài khoản mà không phải xóa dữ liệu lịch sử.
    public bool IsActive { get; set; } = true;

    // Ghi lại thời điểm tạo tài khoản.
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Danh sách các lần thi của người dùng này.
    public ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
}
