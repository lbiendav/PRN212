using ExamManagementSystem.DataAccess.Entities;

namespace ExamManagementSystem.DataAccess.Repositories;

/// <summary>
/// Khai báo thao tác dữ liệu dành cho đề thi.
/// </summary>
public interface IExamRepository
{
    // Lấy danh sách đề, có hỗ trợ tìm kiếm/lọc và chọn chỉ đề hoạt động.
    Task<List<Exam>> SearchAsync(string keyword, string subject, bool? isActive);

    // Lấy toàn bộ câu hỏi và phương án của một đề để bắt đầu thi.
    Task<Exam?> GetFullExamAsync(int id);

    // Lấy thông tin cơ bản của đề theo Id.
    Task<Exam?> GetByIdAsync(int id);

    // Thêm đề thi mới.
    Task<Exam> AddAsync(Exam exam);

    // Cập nhật thông tin cơ bản của đề.
    Task UpdateAsync(Exam exam);

    // Xóa đề nếu chưa có lượt thi.
    Task<bool> DeleteAsync(int id);
}
