using System.Text.RegularExpressions;
using ExamManagementSystem.Business.DTOs;
using ExamManagementSystem.Business.Security;
using ExamManagementSystem.DataAccess.Entities;
using ExamManagementSystem.DataAccess.Repositories;

namespace ExamManagementSystem.Business.Services;

/// <summary>
/// Chứa nghiệp vụ đăng ký và đăng nhập, tách khỏi giao diện WPF.
/// </summary>
public class AuthService(IUserRepository userRepository, PasswordHasher passwordHasher)
{
    // Hai dependency được truyền vào giúp lớp dễ kiểm thử và không tự truy cập database.
    private readonly IUserRepository _userRepository = userRepository;
    private readonly PasswordHasher _passwordHasher = passwordHasher;

    /// <summary>
    /// Kiểm tra tài khoản và trả về phiên đăng nhập nếu hợp lệ.
    /// </summary>
    public async Task<ServiceResult<UserSession>> LoginAsync(string username, string password)
    {
        // Trim username để tránh lỗi do người dùng vô tình gõ khoảng trắng.
        username = username.Trim();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return ServiceResult<UserSession>.Failure("Vui lòng nhập tên đăng nhập và mật khẩu.");
        }

        var user = await _userRepository.FindByUsernameAsync(username);
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            return ServiceResult<UserSession>.Failure("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        if (!user.IsActive)
        {
            return ServiceResult<UserSession>.Failure("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
        }

        // Không đưa PasswordHash vào session để tránh rò rỉ ra tầng Presentation.
        var session = new UserSession(user.Id, user.Username, user.FullName, user.Role);
        return ServiceResult<UserSession>.Success(session, "Đăng nhập thành công.");
    }

    /// <summary>
    /// Kiểm tra dữ liệu và tạo tài khoản Student mới.
    /// </summary>
    public async Task<ServiceResult<UserSession>> RegisterAsync(
        string username,
        string fullName,
        string email,
        string password,
        string confirmPassword)
    {
        // Chuẩn hóa dữ liệu trước khi validation và lưu.
        username = username.Trim();
        fullName = fullName.Trim();
        email = email.Trim().ToLowerInvariant();

        if (username.Length is < 4 or > 50 || !Regex.IsMatch(username, "^[a-zA-Z0-9_]+$"))
        {
            return ServiceResult<UserSession>.Failure("Tên đăng nhập dài 4-50 ký tự, chỉ gồm chữ, số và dấu gạch dưới.");
        }

        if (fullName.Length is < 2 or > 100)
        {
            return ServiceResult<UserSession>.Failure("Họ tên phải dài từ 2 đến 100 ký tự.");
        }

        if (!Regex.IsMatch(email, "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$") || email.Length > 120)
        {
            return ServiceResult<UserSession>.Failure("Email không đúng định dạng.");
        }

        if (password.Length < 6)
        {
            return ServiceResult<UserSession>.Failure("Mật khẩu phải có ít nhất 6 ký tự.");
        }

        if (password != confirmPassword)
        {
            return ServiceResult<UserSession>.Failure("Mật khẩu xác nhận không khớp.");
        }

        if (await _userRepository.ExistsAsync(username, email))
        {
            return ServiceResult<UserSession>.Failure("Tên đăng nhập hoặc email đã được sử dụng.");
        }

        // Tài khoản tự đăng ký luôn là Student để không thể tự cấp quyền Admin.
        var user = await _userRepository.AddAsync(new User
        {
            Username = username,
            FullName = fullName,
            Email = email,
            PasswordHash = _passwordHasher.Hash(password),
            Role = "Student",
            IsActive = true,
            CreatedAt = DateTime.Now
        });

        return ServiceResult<UserSession>.Success(
            new UserSession(user.Id, user.Username, user.FullName, user.Role),
            "Đăng ký thành công.");
    }
}
