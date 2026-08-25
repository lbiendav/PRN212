namespace ExamManagementSystem.Business.DTOs;

/// <summary>
/// Thông tin tối thiểu của người đang đăng nhập, được giữ trong bộ nhớ ứng dụng.
/// </summary>
public record UserSession(int Id, string Username, string FullName, string Role);

/// <summary>
/// Dữ liệu hiển thị một dòng trong bảng quản lý người dùng.
/// </summary>
public class UserListItemDto
{
    // Khóa chính dùng khi sửa hoặc xóa.
    public int Id { get; init; }

    // Các thuộc tính hiển thị trong DataGrid.
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }

    // Chuyển bool thành chữ tiếng Việt cho người mới dễ đọc.
    public string StatusText => IsActive ? "Đang hoạt động" : "Đã khóa";
}

/// <summary>
/// Dữ liệu quản trị viên nhập khi thêm hoặc sửa tài khoản.
/// </summary>
public class UserEditDto
{
    // Id bằng 0 nghĩa là tạo mới.
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Student";
    public bool IsActive { get; set; } = true;

    // Khi sửa, để trống Password nghĩa là giữ mật khẩu cũ.
    public string Password { get; set; } = string.Empty;
}
