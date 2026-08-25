using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using ExamManagementSystem.Business.DTOs;
using ExamManagementSystem.Business.Services;
using ExamManagementSystem.Presentation.MVVM;

namespace ExamManagementSystem.Presentation.ViewModels;

/// <summary>
/// ViewModel màn hình làm bài, quản lý lựa chọn và bộ đếm thời gian.
/// </summary>
public class ExamTakingViewModel : ViewModelBase, IDisposable
{
    // Service, phiên đăng nhập và callback điều hướng sau khi nộp.
    private readonly ExamService _examService;
    private readonly UserSession _session;
    private readonly Action<int> _showResult;
    private readonly Action _cancel;
    private readonly DispatcherTimer _timer;
    private readonly DateTime _startedAt = DateTime.Now;
    private TimeSpan _remainingTime;
    private bool _isSubmitted;
    private string _message = string.Empty;

    /// <summary>
    /// Chuyển DTO thành các câu hỏi có trạng thái lựa chọn riêng cho giao diện.
    /// </summary>
    public ExamTakingViewModel(
        ExamService examService,
        UserSession session,
        ExamTakingDto exam,
        Action<int> showResult,
        Action cancel)
    {
        _examService = examService;
        _session = session;
        _showResult = showResult;
        _cancel = cancel;
        ExamId = exam.Id;
        Title = exam.Title;
        Subject = exam.Subject;
        Questions = new ObservableCollection<QuestionAnswerViewModel>(
            exam.Questions.Select(x => new QuestionAnswerViewModel(x)));

        SubmitCommand = new AsyncRelayCommand(_ => SubmitAsync());
        CancelCommand = new RelayCommand(_ => _cancel(), _ => !_isSubmitted);

        // DispatcherTimer chạy trên UI thread nên có thể cập nhật Binding trực tiếp.
        _remainingTime = TimeSpan.FromMinutes(exam.DurationMinutes);
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += TimerTick;
        _timer.Start();
    }

    // Thông tin đầu đề và danh sách câu hỏi hiển thị.
    public int ExamId { get; }
    public string Title { get; }
    public string Subject { get; }
    public ObservableCollection<QuestionAnswerViewModel> Questions { get; }
    public string RemainingTimeText => $"Còn lại: {_remainingTime:mm\\:ss}";
    public string Message { get => _message; set => SetProperty(ref _message, value); }

    // Lệnh nộp và hủy bài.
    public ICommand SubmitCommand { get; }
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Giảm thời gian mỗi giây và tự nộp khi đồng hồ về 0.
    /// </summary>
    private void TimerTick(object? sender, EventArgs e)
    {
        _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
        if (_remainingTime <= TimeSpan.Zero)
        {
            _remainingTime = TimeSpan.Zero;
            _timer.Stop();
            // Gọi qua command để dùng chung cơ chế chống bấm lặp và bắt lỗi.
            SubmitCommand.Execute(null);
        }
        OnPropertyChanged(nameof(RemainingTimeText));
    }

    /// <summary>
    /// Gom lựa chọn theo QuestionId rồi gửi sang Business để chấm và lưu.
    /// </summary>
    private async Task SubmitAsync()
    {
        if (_isSubmitted) return;
        _isSubmitted = true;
        _timer.Stop();

        var selections = Questions.ToDictionary(x => x.Id, x => x.SelectedOptionId);
        var result = await _examService.SubmitAsync(_session.Id, ExamId, _startedAt, selections);
        if (!result.IsSuccess || result.Data is null)
        {
            // Cho phép thử nộp lại nếu lỗi nghiệp vụ tạm thời.
            _isSubmitted = false;
            _timer.Start();
            Message = result.Message;
            return;
        }

        _showResult(result.Data.AttemptId);
    }

    /// <summary>
    /// Dừng timer khi MainViewModel rời khỏi màn hình làm bài.
    /// </summary>
    public void Dispose() => _timer.Stop();
}

/// <summary>
/// Trạng thái trả lời của một câu hỏi trên UI.
/// </summary>
public class QuestionAnswerViewModel
{
    /// <summary>
    /// Tạo danh sách OptionSelectionViewModel và liên kết hành vi chọn một đáp án.
    /// </summary>
    public QuestionAnswerViewModel(QuestionDto question)
    {
        Id = question.Id;
        OrderNumber = question.OrderNumber;
        Content = question.Content;
        Options = new ObservableCollection<OptionSelectionViewModel>(
            question.Options.Select(option => new OptionSelectionViewModel(option, SelectOption)));
    }

    // Dữ liệu câu hỏi và Id phương án đã chọn.
    public int Id { get; }
    public int OrderNumber { get; }
    public string Content { get; }
    public ObservableCollection<OptionSelectionViewModel> Options { get; }
    public int? SelectedOptionId { get; private set; }

    /// <summary>
    /// Bảo đảm trong một câu chỉ có đúng một RadioButton được chọn.
    /// </summary>
    private void SelectOption(OptionSelectionViewModel selected)
    {
        SelectedOptionId = selected.Id;
        foreach (var option in Options)
        {
            option.SetSelected(option.Id == selected.Id);
        }
    }
}

/// <summary>
/// Trạng thái của một RadioButton phương án trả lời.
/// </summary>
public class OptionSelectionViewModel(AnswerOptionDto option, Action<OptionSelectionViewModel> onSelected) : ViewModelBase
{
    // Callback báo cho câu hỏi khi phương án này được chọn.
    private readonly Action<OptionSelectionViewModel> _onSelected = onSelected;
    private bool _isSelected;

    // Id/nội dung chỉ đọc lấy từ DTO.
    public int Id { get; } = option.Id;
    public string Content { get; } = option.Content;

    // Setter gọi callback; SetSelected nội bộ tránh callback lặp khi bỏ chọn đáp án khác.
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (value && SetProperty(ref _isSelected, true)) _onSelected(this);
        }
    }

    /// <summary>
    /// Đồng bộ trạng thái RadioButton từ ViewModel câu hỏi.
    /// </summary>
    public void SetSelected(bool value) => SetProperty(ref _isSelected, value, nameof(IsSelected));
}
