using Microsoft.EntityFrameworkCore;

namespace ExamManagementSystem.DataAccess;

/// <summary>
/// Tạo DbContext mới cho mỗi thao tác để tránh giữ kết nối cơ sở dữ liệu quá lâu.
/// </summary>
public class ExamDbContextFactory(string connectionString)
{
    // Lưu chuỗi kết nối được đọc từ appsettings.json.
    private readonly string _connectionString = connectionString;

    /// <summary>
    /// Khởi tạo một DbContext đã được cấu hình dùng SQL Server.
    /// </summary>
    public ExamDbContext Create()
    {
        // DbContextOptions cho EF Core biết nhà cung cấp và chuỗi kết nối cần dùng.
        var options = new DbContextOptionsBuilder<ExamDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        return new ExamDbContext(options);
    }
}
