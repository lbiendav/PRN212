using ExamManagementSystem.DataAccess.Entities;

namespace ExamManagementSystem.DataAccess.Repositories;

/// <summary>
/// Khai báo các thao tác dữ liệu tài khoản mà tầng Business được phép dùng.
/// </summary>
public interface IUserRepository
{
    // Tìm tài khoản bằng tên đăng nhập để xác thực.
    Task<User?> FindByUsernameAsync(string username);

    // Kiểm tra username hoặc email đã tồn tại hay chưa.
    Task<bool> ExistsAsync(string username, string email, int? ignoredUserId = null);

    // Thêm tài khoản mới và trả về bản ghi đã có Id.
    Task<User> AddAsync(User user);

    // Lấy một tài khoản theo khóa chính.
    Task<User?> GetByIdAsync(int id);

    // Tìm kiếm và lọc danh sách tài khoản cho trang quản trị.
    Task<List<User>> SearchAsync(string keyword, string role, bool? isActive);

    // Cập nhật thông tin một tài khoản.
    Task UpdateAsync(User user);

    // Xóa tài khoản chưa có lịch sử thi; trả false nếu không thể xóa.
    Task<bool> DeleteAsync(int id);
}
