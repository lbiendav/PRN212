using System.Collections.ObjectModel;
using System.Windows.Input;
using ExamManagementSystem.Business.DTOs;
using ExamManagementSystem.Presentation.MVVM;

namespace ExamManagementSystem.Presentation.ViewModels;

/// <summary>
/// ViewModel hiển thị điểm và từng đáp án đúng/sai của một lần thi.
/// </summary>
public class ReviewViewModel
{
    /// <summary>
    /// Nhận dữ liệu đã xử lý từ Business và tạo các lệnh điều hướng.
    /// </summary>
    public ReviewViewModel(AttemptReviewDto review, Action goHome, Action<int> retake)
    {
        AttemptId = review.AttemptId;
        ExamId = review.ExamId;
        ExamTitle = review.ExamTitle;
        ScoreText = $"{review.Score:0.##}/10";
        SummaryText = $"Đúng {review.CorrectAnswers}/{review.TotalQuestions} câu • Nộp lúc {review.SubmittedAt:dd/MM/yyyy HH:mm}";
        Answers = new ObservableCollection<AnswerReviewDto>(review.Answers);
        GoHomeCommand = new RelayCommand(_ => goHome());
        RetakeCommand = new RelayCommand(_ => retake(ExamId));
    }

    // Thuộc tính chỉ đọc dùng cho phần đầu và danh sách chi tiết.
    public int AttemptId { get; }
    public int ExamId { get; }
    public string ExamTitle { get; }
    public string ScoreText { get; }
    public string SummaryText { get; }
    public ObservableCollection<AnswerReviewDto> Answers { get; }

    // Các nút trở về và thi lại.
    public ICommand GoHomeCommand { get; }
    public ICommand RetakeCommand { get; }
}
