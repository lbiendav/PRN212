using ExamManagementSystem.Business.DTOs;
using ExamManagementSystem.DataAccess.Entities;
using ExamManagementSystem.DataAccess.Repositories;

namespace ExamManagementSystem.Business.Services;

/// <summary>
/// Xử lý nghiệp vụ danh sách đề, nộp bài, chấm điểm và xem lịch sử.
/// </summary>
public class ExamService(IExamRepository examRepository, IAttemptRepository attemptRepository)
{
    // Repository giúp Business không phụ thuộc trực tiếp cách EF Core truy vấn.
    private readonly IExamRepository _examRepository = examRepository;
    private readonly IAttemptRepository _attemptRepository = attemptRepository;

    /// <summary>
    /// Lấy các đề đang mở cho trang chủ học viên.
    /// </summary>
    public async Task<List<ExamListItemDto>> GetAvailableExamsAsync(string keyword = "", string subject = "")
    {
        var exams = await _examRepository.SearchAsync(keyword.Trim(), subject, true);

        // Map Entity sang DTO để tầng Presentation không làm việc trực tiếp với Entity.
        return exams.Select(ToListItem).ToList();
    }

    /// <summary>
    /// Chuẩn bị câu hỏi và phương án nhưng tuyệt đối không gửi cờ đáp án đúng ra UI.
    /// </summary>
    public async Task<ServiceResult<ExamTakingDto>> StartExamAsync(int examId)
    {
        var exam = await _examRepository.GetFullExamAsync(examId);
        if (exam is null || !exam.IsActive)
        {
            return ServiceResult<ExamTakingDto>.Failure("Đề thi không tồn tại hoặc đang bị ẩn.");
        }

        if (exam.Questions.Count == 0)
        {
            return ServiceResult<ExamTakingDto>.Failure("Đề thi chưa có câu hỏi.");
        }

        // Chỉ map Id và nội dung phương án để đáp án đúng vẫn nằm ở tầng nghiệp vụ.
        var questions = exam.Questions.OrderBy(x => x.OrderNumber)
            .Select(q => new QuestionDto(
                q.Id,
                q.OrderNumber,
                q.Content,
                q.Options.OrderBy(x => x.Id)
                    .Select(x => new AnswerOptionDto(x.Id, x.Content)).ToList()))
            .ToList();

        var dto = new ExamTakingDto(exam.Id, exam.Title, exam.Subject, exam.DurationMinutes, questions);
        return ServiceResult<ExamTakingDto>.Success(dto);
    }

    /// <summary>
    /// Đối chiếu lựa chọn với đáp án đúng, tính điểm thang 10 và lưu lịch sử.
    /// </summary>
    public async Task<ServiceResult<SubmitResultDto>> SubmitAsync(
        int userId,
        int examId,
        DateTime startedAt,
        IReadOnlyDictionary<int, int?> selectedAnswers)
    {
        var exam = await _examRepository.GetFullExamAsync(examId);
        if (exam is null || exam.Questions.Count == 0)
        {
            return ServiceResult<SubmitResultDto>.Failure("Không thể chấm điểm vì đề thi không hợp lệ.");
        }

        // Mỗi câu tạo một AttemptAnswer, kể cả câu người dùng chưa trả lời.
        var answerEntities = new List<AttemptAnswer>();
        foreach (var question in exam.Questions)
        {
            selectedAnswers.TryGetValue(question.Id, out var selectedOptionId);

            // Chỉ chấp nhận Option thật sự thuộc câu này để chống dữ liệu giả mạo.
            var selectedOption = question.Options.FirstOrDefault(x => x.Id == selectedOptionId);
            var isCorrect = selectedOption?.IsCorrect == true;
            answerEntities.Add(new AttemptAnswer
            {
                QuestionId = question.Id,
                SelectedOptionId = selectedOption?.Id,
                IsCorrect = isCorrect
            });
        }

        var correctCount = answerEntities.Count(x => x.IsCorrect);
        var totalCount = exam.Questions.Count;

        // Làm tròn hai chữ số để điểm như 6.67 hiển thị nhất quán.
        var score = Math.Round(correctCount * 10m / totalCount, 2, MidpointRounding.AwayFromZero);
        var attempt = await _attemptRepository.AddAsync(new ExamAttempt
        {
            UserId = userId,
            ExamId = examId,
            CorrectAnswers = correctCount,
            TotalQuestions = totalCount,
            Score = score,
            StartedAt = startedAt,
            SubmittedAt = DateTime.Now,
            Answers = answerEntities
        });

        var result = new SubmitResultDto(attempt.Id, score, correctCount, totalCount);
        return ServiceResult<SubmitResultDto>.Success(result, "Nộp bài thành công.");
    }

    /// <summary>
    /// Lấy lịch sử của đúng người dùng đang đăng nhập.
    /// </summary>
    public async Task<List<AttemptHistoryDto>> GetHistoryAsync(int userId)
    {
        var attempts = await _attemptRepository.GetHistoryAsync(userId);
        return attempts.Select(x => new AttemptHistoryDto
        {
            Id = x.Id,
            ExamId = x.ExamId,
            ExamTitle = x.Exam.Title,
            Subject = x.Exam.Subject,
            Score = x.Score,
            CorrectAnswers = x.CorrectAnswers,
            TotalQuestions = x.TotalQuestions,
            SubmittedAt = x.SubmittedAt
        }).ToList();
    }

    /// <summary>
    /// Tạo dữ liệu xem lại, chỉ cho phép chủ sở hữu lần thi đọc chi tiết.
    /// </summary>
    public async Task<ServiceResult<AttemptReviewDto>> GetReviewAsync(int attemptId, int userId)
    {
        var attempt = await _attemptRepository.GetDetailAsync(attemptId, userId);
        if (attempt is null)
        {
            return ServiceResult<AttemptReviewDto>.Failure("Không tìm thấy lần thi cần xem.");
        }

        // Tìm phương án IsCorrect để hiển thị đáp án chuẩn cạnh lựa chọn của học viên.
        var answers = attempt.Answers.OrderBy(x => x.Question.OrderNumber)
            .Select(x => new AnswerReviewDto
            {
                OrderNumber = x.Question.OrderNumber,
                QuestionContent = x.Question.Content,
                SelectedAnswer = x.SelectedOption?.Content ?? "Chưa trả lời",
                CorrectAnswer = x.Question.Options.FirstOrDefault(o => o.IsCorrect)?.Content ?? "Chưa cấu hình",
                IsCorrect = x.IsCorrect
            }).ToList();

        var dto = new AttemptReviewDto(
            attempt.Id,
            attempt.ExamId,
            attempt.Exam.Title,
            attempt.Score,
            attempt.CorrectAnswers,
            attempt.TotalQuestions,
            attempt.SubmittedAt,
            answers);
        return ServiceResult<AttemptReviewDto>.Success(dto);
    }

    /// <summary>
    /// Hàm map dùng chung cho danh sách đề của học viên và quản trị viên.
    /// </summary>
    internal static ExamListItemDto ToListItem(Exam exam) => new()
    {
        Id = exam.Id,
        Title = exam.Title,
        Subject = exam.Subject,
        Description = exam.Description,
        DurationMinutes = exam.DurationMinutes,
        QuestionCount = exam.Questions.Count,
        IsActive = exam.IsActive
    };
}
