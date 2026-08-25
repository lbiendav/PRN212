using ExamManagementSystem.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamManagementSystem.DataAccess;

/// <summary>
/// DbContext là cầu nối giữa các đối tượng C# và bảng trong SQL Server.
/// </summary>
public class ExamDbContext(DbContextOptions<ExamDbContext> options) : DbContext(options)
{
    // Mỗi DbSet tương ứng với một bảng trong cơ sở dữ liệu.
    public DbSet<User> Users => Set<User>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();

    /// <summary>
    /// Cấu hình khóa, độ dài cột, quan hệ và quy tắc xóa giữa các bảng.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Username và Email là duy nhất để không có hai tài khoản trùng nhau.
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(120).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(20).IsRequired();
        });

        // Thiết lập thuộc tính quan trọng của đề thi.
        modelBuilder.Entity<Exam>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        // Khi xóa đề, câu hỏi và phương án của đề cũng được xóa theo.
        modelBuilder.Entity<Question>()
            .HasOne(x => x.Exam).WithMany(x => x.Questions)
            .HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AnswerOption>()
            .HasOne(x => x.Question).WithMany(x => x.Options)
            .HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);

        // Không cho xóa User/Exam nếu đã có lịch sử thi để bảo toàn dữ liệu.
        modelBuilder.Entity<ExamAttempt>()
            .HasOne(x => x.User).WithMany(x => x.ExamAttempts)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ExamAttempt>()
            .HasOne(x => x.Exam).WithMany(x => x.ExamAttempts)
            .HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ExamAttempt>()
            .Property(x => x.Score).HasPrecision(4, 2);

        // SelectedOption là quan hệ tùy chọn vì học viên có thể bỏ trống câu hỏi.
        modelBuilder.Entity<AttemptAnswer>()
            .HasOne(x => x.SelectedOption).WithMany()
            .HasForeignKey(x => x.SelectedOptionId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<AttemptAnswer>()
            .HasOne(x => x.Question).WithMany()
            .HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.NoAction);
    }
}
