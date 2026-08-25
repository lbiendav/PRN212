using ExamManagementSystem.DataAccess.Entities;

namespace ExamManagementSystem.DataAccess.Repositories;

/// <summary>
/// Khai báo các thao tác lưu và đọc lịch sử làm bài.
/// </summary>
public interface IAttemptRepository
{
    // Lưu một lần nộp bài cùng toàn bộ câu trả lời chi tiết.
    Task<ExamAttempt> AddAsync(ExamAttempt attempt);

    // Lấy tất cả lần thi của một học viên.
    Task<List<ExamAttempt>> GetHistoryAsync(int userId);

    // Lấy một lần thi đầy đủ để hiển thị đáp án đúng/sai.
    Task<ExamAttempt?> GetDetailAsync(int attemptId, int userId);
}
