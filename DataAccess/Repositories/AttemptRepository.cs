using ExamManagementSystem.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamManagementSystem.DataAccess.Repositories;

/// <summary>
/// Hiện thực kho dữ liệu lịch sử thi.
/// </summary>
public class AttemptRepository(ExamDbContextFactory contextFactory) : IAttemptRepository
{
    // Factory tạo DbContext ngắn hạn và an toàn giữa các màn hình.
    private readonly ExamDbContextFactory _contextFactory = contextFactory;

    /// <inheritdoc />
    public async Task<ExamAttempt> AddAsync(ExamAttempt attempt)
    {
        await using var context = _contextFactory.Create();
        context.ExamAttempts.Add(attempt);
        await context.SaveChangesAsync();
        return attempt;
    }

    /// <inheritdoc />
    public async Task<List<ExamAttempt>> GetHistoryAsync(int userId)
    {
        await using var context = _contextFactory.Create();

        // Include Exam để giao diện có tên môn mà không cần truy vấn lần nữa.
        return await context.ExamAttempts.AsNoTracking()
            .Include(x => x.Exam)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<ExamAttempt?> GetDetailAsync(int attemptId, int userId)
    {
        await using var context = _contextFactory.Create();

        // Nạp đủ câu hỏi, phương án đúng và lựa chọn để tạo màn hình xem lại.
        return await context.ExamAttempts.AsNoTracking()
            .Include(x => x.Exam)
            .Include(x => x.Answers)
                .ThenInclude(x => x.Question)
                    .ThenInclude(x => x.Options)
            .Include(x => x.Answers)
                .ThenInclude(x => x.SelectedOption)
            .FirstOrDefaultAsync(x => x.Id == attemptId && x.UserId == userId);
    }
}
