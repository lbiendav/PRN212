using System.Collections.ObjectModel;
using System.Windows.Input;
using ExamManagementSystem.Business.DTOs;
using ExamManagementSystem.Business.Services;
using ExamManagementSystem.Presentation.MVVM;

namespace ExamManagementSystem.Presentation.ViewModels;

/// <summary>
/// ViewModel trang chủ học viên, chứa danh sách đề và lịch sử làm bài.
/// </summary>
public class HomeViewModel : ViewModelBase
{
    // Dependency, session và callback điều hướng sang trang làm/xem lại bài.
    private readonly ExamService _examService;
    private readonly UserSession _session;
    private readonly Action<int> _startExam;
    private readonly Action<int> _reviewAttempt;
    private string _searchText = string.Empty;
    private string _welcomeText = string.Empty;

    /// <summary>
    /// Khởi tạo command và tải dữ liệu ngay khi mở trang.
    /// </summary>
    public HomeViewModel(
        ExamService examService,
        UserSession session,
        Action<int> startExam,
        Action<int> reviewAttempt,
        Action logout)
    {
        _examService = examService;
        _session = session;
        _startExam = startExam;
        _reviewAttempt = reviewAttempt;
        WelcomeText = $"Xin chào, {session.FullName}";

        SearchCommand = new AsyncRelayCommand(_ => LoadExamsAsync());
        RefreshCommand = new AsyncRelayCommand(_ => LoadAsync());
        StartExamCommand = new RelayCommand(x => StartExam(x));
        ReviewCommand = new RelayCommand(x => Review(x));
        RetakeCommand = new RelayCommand(x => Retake(x));
        LogoutCommand = new RelayCommand(_ => logout());

        // Gọi qua AsyncRelayCommand để lỗi kết nối ban đầu cũng được bắt và thông báo.
        RefreshCommand.Execute(null);
    }

    // ObservableCollection tự báo cho ItemsControl/DataGrid khi dữ liệu được thay mới.
    public ObservableCollection<ExamListItemDto> Exams { get; } = [];
    public ObservableCollection<AttemptHistoryDto> History { get; } = [];

    // Dữ liệu hiển thị và ô tìm kiếm của trang chủ.
    public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }
    public string WelcomeText { get => _welcomeText; set => SetProperty(ref _welcomeText, value); }

    // Các lệnh giao diện có thể Binding.
    public ICommand SearchCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand StartExamCommand { get; }
    public ICommand ReviewCommand { get; }
    public ICommand RetakeCommand { get; }
    public ICommand LogoutCommand { get; }

    /// <summary>
    /// Tải song song đề thi và lịch sử để giảm thời gian chờ.
    /// </summary>
    private async Task LoadAsync()
    {
        var examsTask = _examService.GetAvailableExamsAsync(SearchText);
        var historyTask = _examService.GetHistoryAsync(_session.Id);
        await Task.WhenAll(examsTask, historyTask);

        ReplaceItems(Exams, examsTask.Result);
        ReplaceItems(History, historyTask.Result);
    }

    /// <summary>
    /// Chỉ tải lại danh sách đề khi người dùng tìm kiếm.
    /// </summary>
    private async Task LoadExamsAsync()
    {
        var exams = await _examService.GetAvailableExamsAsync(SearchText);
        ReplaceItems(Exams, exams);
    }

    // Các hàm dưới kiểm tra đúng kiểu tham số trước khi điều hướng.
    private void StartExam(object? parameter)
    {
        if (parameter is ExamListItemDto exam) _startExam(exam.Id);
    }

    private void Review(object? parameter)
    {
        if (parameter is AttemptHistoryDto attempt) _reviewAttempt(attempt.Id);
    }

    private void Retake(object? parameter)
    {
        if (parameter is AttemptHistoryDto attempt) _startExam(attempt.ExamId);
    }

    /// <summary>
    /// Thay nội dung collection mà không đổi đối tượng đang được Binding.
    /// </summary>
    private static void ReplaceItems<T>(ObservableCollection<T> target, IEnumerable<T> values)
    {
        target.Clear();
        foreach (var value in values) target.Add(value);
    }
}
