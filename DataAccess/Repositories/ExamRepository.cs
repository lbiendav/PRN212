using ExamManagementSystem.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamManagementSystem.DataAccess.Repositories;

/// <summary>
/// Hiện thực kho dữ liệu đề thi bằng Entity Framework Core.
/// </summary>
public class ExamRepository(ExamDbContextFactory contextFactory) : IExamRepository
{
    // Factory tạo DbContext theo từng thao tác.
    private readonly ExamDbContextFactory _contextFactory = contextFactory;

    /// <inheritdoc />
    public async Task<List<Exam>> SearchAsync(string keyword, string subject, bool? isActive)
    {
        await using var context = _contextFactory.Create();
        var query = context.Exams.AsNoTracking().Include(x => x.Questions).AsQueryable();

        // Tìm theo tiêu đề, môn hoặc mô tả đề thi.
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Title.Contains(keyword) ||
                                     x.Subject.Contains(keyword) ||
                                     x.Description.Contains(keyword));
        }

        // Lọc đúng tên môn nếu người dùng đã chọn một môn cụ thể.
        if (!string.IsNullOrWhiteSpace(subject))
        {
            query = query.Where(x => x.Subject == subject);
        }

        // null nghĩa là lấy cả đề đang bật và đang ẩn.
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Exam?> GetFullExamAsync(int id)
    {
        await using var context = _contextFactory.Create();

        // ThenInclude nạp cả phương án; nếu thiếu, Options sẽ là danh sách rỗng.
        return await context.Exams.AsNoTracking()
            .Include(x => x.Questions.OrderBy(q => q.OrderNumber))
            .ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <inheritdoc />
    public async Task<Exam?> GetByIdAsync(int id)
    {
        await using var context = _contextFactory.Create();
        return await context.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <inheritdoc />
    public async Task<Exam> AddAsync(Exam exam)
    {
        await using var context = _contextFactory.Create();
        context.Exams.Add(exam);
        await context.SaveChangesAsync();
        return exam;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Exam exam)
    {
        await using var context = _contextFactory.Create();
        context.Exams.Update(exam);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = _contextFactory.Create();
        var exam = await context.Exams.FindAsync(id);
        if (exam is null || await context.ExamAttempts.AnyAsync(x => x.ExamId == id))
        {
            return false;
        }

        // Quan hệ cascade sẽ tự xóa Questions và AnswerOptions thuộc đề.
        context.Exams.Remove(exam);
        await context.SaveChangesAsync();
        return true;
    }
}
