using ExamManagementSystem.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamManagementSystem.DataAccess.Repositories;

/// <summary>
/// Hiện thực các thao tác bảng Users bằng Entity Framework Core.
/// </summary>
public class UserRepository(ExamDbContextFactory contextFactory) : IUserRepository
{
    // Factory dùng để tạo DbContext riêng cho từng thao tác bất đồng bộ.
    private readonly ExamDbContextFactory _contextFactory = contextFactory;

    /// <inheritdoc />
    public async Task<User?> FindByUsernameAsync(string username)
    {
        // AsNoTracking phù hợp vì dữ liệu chỉ được đọc, không sửa trong DbContext này.
        await using var context = _contextFactory.Create();
        return await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string username, string email, int? ignoredUserId = null)
    {
        await using var context = _contextFactory.Create();

        // ignoredUserId giúp màn hình sửa không tự báo trùng với chính bản ghi đang sửa.
        return await context.Users.AnyAsync(x =>
            (!ignoredUserId.HasValue || x.Id != ignoredUserId.Value) &&
            (x.Username == username || x.Email == email));
    }

    /// <inheritdoc />
    public async Task<User> AddAsync(User user)
    {
        await using var context = _contextFactory.Create();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(int id)
    {
        await using var context = _contextFactory.Create();
        return await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <inheritdoc />
    public async Task<List<User>> SearchAsync(string keyword, string role, bool? isActive)
    {
        await using var context = _contextFactory.Create();
        var query = context.Users.AsNoTracking().AsQueryable();

        // Tìm gần đúng theo username, họ tên hoặc email.
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Username.Contains(keyword) ||
                                     x.FullName.Contains(keyword) ||
                                     x.Email.Contains(keyword));
        }

        // Giá trị "Tất cả" được tầng giao diện chuyển thành chuỗi rỗng.
        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(x => x.Role == role);
        }

        // null nghĩa là không lọc trạng thái.
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(User user)
    {
        await using var context = _contextFactory.Create();
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = _contextFactory.Create();
        var user = await context.Users.FindAsync(id);
        if (user is null || await context.ExamAttempts.AnyAsync(x => x.UserId == id))
        {
            return false;
        }

        // Chỉ xóa tài khoản không có lịch sử để không làm mất kết quả thi.
        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return true;
    }
}
