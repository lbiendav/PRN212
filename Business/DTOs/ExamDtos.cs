namespace ExamManagementSystem.Business.DTOs;

/// <summary>
/// Dữ liệu tóm tắt của một đề thi trên trang chủ hoặc trang quản trị.
/// </summary>
public class ExamListItemDto
{
    // Id được dùng để mở bài thi hoặc chỉnh sửa.
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int DurationMinutes { get; init; }
    public int QuestionCount { get; init; }
    public bool IsActive { get; init; }

    // Chuỗi này giúp DataGrid hiển thị trạng thái rõ ràng.
    public string StatusText => IsActive ? "Đang mở" : "Đang ẩn";
}

/// <summary>
/// Dữ liệu quản trị viên nhập khi thêm hoặc sửa đề.
/// </summary>
public class ExamEditDto
{
    // Id bằng 0 nghĩa là tạo một đề mới.
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; } = 15;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Một phương án trả lời được gửi tới màn hình làm bài.
/// </summary>
public record AnswerOptionDto(int Id, string Content);

/// <summary>
/// Một câu hỏi kèm các phương án; không chứa IsCorrect để tránh lộ đáp án.
/// </summary>
public record QuestionDto(int Id, int OrderNumber, string Content, List<AnswerOptionDto> Options);

/// <summary>
/// Toàn bộ dữ liệu cần thiết để bắt đầu làm một đề thi.
/// </summary>
public record ExamTakingDto(
    int Id,
    string Title,
    string Subject,
    int DurationMinutes,
    List<QuestionDto> Questions);
