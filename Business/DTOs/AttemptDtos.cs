namespace ExamManagementSystem.Business.DTOs;

/// <summary>
/// Một dòng lịch sử thi của học viên.
/// </summary>
public class AttemptHistoryDto
{
    // Id dùng để mở chi tiết lần thi.
    public int Id { get; init; }
    public int ExamId { get; init; }
    public string ExamTitle { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public decimal Score { get; init; }
    public int CorrectAnswers { get; init; }
    public int TotalQuestions { get; init; }
    public DateTime SubmittedAt { get; init; }

    // Chuỗi tổng hợp để hiển thị gọn trong bảng.
    public string CorrectText => $"{CorrectAnswers}/{TotalQuestions}";
}

/// <summary>
/// Kết quả đúng/sai của một câu trên màn hình xem lại.
/// </summary>
public class AnswerReviewDto
{
    // Nội dung của câu hỏi và các câu trả lời liên quan.
    public int OrderNumber { get; init; }
    public string QuestionContent { get; init; } = string.Empty;
    public string SelectedAnswer { get; init; } = "Chưa trả lời";
    public string CorrectAnswer { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }

    // Dùng trực tiếp trên giao diện để tô nhãn đúng/sai.
    public string ResultText => IsCorrect ? "ĐÚNG" : "SAI";
}

/// <summary>
/// Dữ liệu đầy đủ của một lần thi để học viên xem lại.
/// </summary>
public record AttemptReviewDto(
    int AttemptId,
    int ExamId,
    string ExamTitle,
    decimal Score,
    int CorrectAnswers,
    int TotalQuestions,
    DateTime SubmittedAt,
    List<AnswerReviewDto> Answers);

/// <summary>
/// Kết quả trả về ngay sau khi nộp bài.
/// </summary>
public record SubmitResultDto(int AttemptId, decimal Score, int CorrectAnswers, int TotalQuestions);
