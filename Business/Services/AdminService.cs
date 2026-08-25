using System.Text.RegularExpressions;
using ExamManagementSystem.Business.DTOs;
using ExamManagementSystem.Business.Security;
using ExamManagementSystem.DataAccess.Entities;
using ExamManagementSystem.DataAccess.Repositories;

namespace ExamManagementSystem.Business.Services;

/// <summary>
/// Chứa nghiệp vụ CRUD, tìm kiếm và lọc dành riêng cho quản trị viên.
/// </summary>
public class AdminService(
    IUserRepository userRepository,
    IExamRepository examRepository,
    PasswordHasher passwordHasher)
{
    // Các dependency phục vụ thao tác dữ liệu và băm mật khẩu.
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IExamRepository _examRepository = examRepository;
    private readonly PasswordHasher _passwordHasher = passwordHasher;

    /// <summary>
    /// Tìm kiếm/lọc tài khoản và map sang DTO.
    /// </summary>
    public async Task<List<UserListItemDto>> SearchUsersAsync(string keyword, string role, bool? isActive)
    {
        var users = await _userRepository.SearchAsync(keyword.Trim(), role, isActive);
        return users.Select(x => new UserListItemDto
        {
            Id = x.Id,
            Username = x.Username,
            FullName = x.FullName,
            Email = x.Email,
            Role = x.Role,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// Lấy thông tin hiện tại để đổ vào form sửa tài khoản.
    /// </summary>
    public async Task<UserEditDto?> GetUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user is null ? null : new UserEditDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }

    /// <summary>
    /// Thêm hoặc cập nhật tài khoản sau khi kiểm tra dữ liệu.
    /// </summary>
    public async Task<ServiceResult<bool>> SaveUserAsync(UserEditDto dto)
    {
        // Chuẩn hóa dữ liệu form trước khi kiểm tra.
        dto.Username = dto.Username.Trim();
        dto.FullName = dto.FullName.Trim();
        dto.Email = dto.Email.Trim().ToLowerInvariant();

        if (dto.Username.Length is < 4 or > 50 || !Regex.IsMatch(dto.Username, "^[a-zA-Z0-9_]+$"))
        {
            return ServiceResult<bool>.Failure("Tên đăng nhập dài 4-50 ký tự, chỉ gồm chữ, số và dấu gạch dưới.");
        }

        if (dto.FullName.Length is < 2 or > 100 ||
            !Regex.IsMatch(dto.Email, "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$"))
        {
            return ServiceResult<bool>.Failure("Họ tên hoặc email không hợp lệ.");
        }

        if (dto.Role is not ("Student" or "Admin"))
        {
            return ServiceResult<bool>.Failure("Vai trò chỉ có thể là Student hoặc Admin.");
        }

        if (await _userRepository.ExistsAsync(dto.Username, dto.Email, dto.Id == 0 ? null : dto.Id))
        {
            return ServiceResult<bool>.Failure("Tên đăng nhập hoặc email đã tồn tại.");
        }

        if (dto.Id == 0)
        {
            if (dto.Password.Length < 6)
            {
                return ServiceResult<bool>.Failure("Tài khoản mới cần mật khẩu ít nhất 6 ký tự.");
            }

            await _userRepository.AddAsync(new User
            {
                Username = dto.Username,
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role,
                IsActive = dto.IsActive,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                CreatedAt = DateTime.Now
            });
        }
        else
        {
            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user is null)
            {
                return ServiceResult<bool>.Failure("Tài khoản cần sửa không còn tồn tại.");
            }

            // Chỉ thay PasswordHash khi quản trị viên thật sự nhập mật khẩu mới.
            user.Username = dto.Username;
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Role = dto.Role;
            user.IsActive = dto.IsActive;
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 6)
                {
                    return ServiceResult<bool>.Failure("Mật khẩu mới phải có ít nhất 6 ký tự.");
                }
                user.PasswordHash = _passwordHasher.Hash(dto.Password);
            }
            await _userRepository.UpdateAsync(user);
        }

        return ServiceResult<bool>.Success(true, "Đã lưu tài khoản.");
    }

    /// <summary>
    /// Xóa tài khoản nếu không phải tài khoản hiện tại và chưa có lịch sử thi.
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteUserAsync(int id, int currentAdminId)
    {
        if (id == currentAdminId)
        {
            return ServiceResult<bool>.Failure("Bạn không thể xóa chính tài khoản đang đăng nhập.");
        }

        var deleted = await _userRepository.DeleteAsync(id);
        return deleted
            ? ServiceResult<bool>.Success(true, "Đã xóa tài khoản.")
            : ServiceResult<bool>.Failure("Không thể xóa tài khoản đã có lịch sử thi. Hãy khóa tài khoản thay vì xóa.");
    }

    /// <summary>
    /// Tìm kiếm và lọc đề thi cho trang quản trị.
    /// </summary>
    public async Task<List<ExamListItemDto>> SearchExamsAsync(string keyword, string subject, bool? isActive)
    {
        var exams = await _examRepository.SearchAsync(keyword.Trim(), subject, isActive);
        return exams.Select(ExamService.ToListItem).ToList();
    }

    /// <summary>
    /// Lấy dữ liệu một đề để đổ vào form sửa.
    /// </summary>
    public async Task<ExamEditDto?> GetExamAsync(int id)
    {
        var exam = await _examRepository.GetByIdAsync(id);
        return exam is null ? null : new ExamEditDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Subject = exam.Subject,
            Description = exam.Description,
            DurationMinutes = exam.DurationMinutes,
            IsActive = exam.IsActive
        };
    }

    /// <summary>
    /// Thêm hoặc cập nhật thông tin cơ bản của đề thi.
    /// </summary>
    public async Task<ServiceResult<bool>> SaveExamAsync(ExamEditDto dto)
    {
        // Kiểm tra các trường bắt buộc và giới hạn phù hợp với database.
        dto.Title = dto.Title.Trim();
        dto.Subject = dto.Subject.Trim();
        dto.Description = dto.Description.Trim();
        if (dto.Title.Length is < 3 or > 150 || dto.Subject.Length is < 2 or > 80)
        {
            return ServiceResult<bool>.Failure("Tên đề hoặc môn học không hợp lệ.");
        }

        if (dto.Description.Length > 500 || dto.DurationMinutes is < 1 or > 300)
        {
            return ServiceResult<bool>.Failure("Mô tả tối đa 500 ký tự và thời gian từ 1 đến 300 phút.");
        }

        if (dto.Id == 0)
        {
            // Đề mới chưa có câu hỏi nên mặc định nên để ẩn cho tới khi thêm dữ liệu SQL.
            await _examRepository.AddAsync(new Exam
            {
                Title = dto.Title,
                Subject = dto.Subject,
                Description = dto.Description,
                DurationMinutes = dto.DurationMinutes,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.Now
            });
        }
        else
        {
            var exam = await _examRepository.GetByIdAsync(dto.Id);
            if (exam is null)
            {
                return ServiceResult<bool>.Failure("Đề thi cần sửa không còn tồn tại.");
            }

            exam.Title = dto.Title;
            exam.Subject = dto.Subject;
            exam.Description = dto.Description;
            exam.DurationMinutes = dto.DurationMinutes;
            exam.IsActive = dto.IsActive;
            await _examRepository.UpdateAsync(exam);
        }

        return ServiceResult<bool>.Success(true, "Đã lưu đề thi.");
    }

    /// <summary>
    /// Xóa đề chưa có lịch sử thi; đề đã được làm chỉ nên chuyển sang trạng thái ẩn.
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteExamAsync(int id)
    {
        var deleted = await _examRepository.DeleteAsync(id);
        return deleted
            ? ServiceResult<bool>.Success(true, "Đã xóa đề thi.")
            : ServiceResult<bool>.Failure("Không thể xóa đề đã có lượt thi. Hãy chuyển đề sang trạng thái ẩn.");
    }
}
