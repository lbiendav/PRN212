using System.Windows;
using ExamManagementSystem.Business.DTOs;
using ExamManagementSystem.Business.Services;
using ExamManagementSystem.Presentation.MVVM;

namespace ExamManagementSystem.Presentation.ViewModels;

/// <summary>
/// ViewModel gốc chịu trách nhiệm đổi màn hình và giữ phiên đăng nhập hiện tại.
/// </summary>
public class MainViewModel : ViewModelBase
{
    // Các service được tái sử dụng giữa những màn hình con.
    private readonly AuthService _authService;
    private readonly ExamService _examService;
    private readonly AdminService _adminService;
    private object? _currentViewModel;
    private UserSession? _currentSession;

    /// <summary>
    /// Nhận dependency từ App và mở màn hình đăng nhập đầu tiên.
    /// </summary>
    public MainViewModel(AuthService authService, ExamService examService, AdminService adminService)
    {
        _authService = authService;
        _examService = examService;
        _adminService = adminService;
        ShowLogin();
    }

    // ContentControl trong MainWindow hiển thị View tương ứng với ViewModel này.
    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            // Dừng timer của màn hình thi cũ trước khi chuyển màn hình.
            if (_currentViewModel is IDisposable disposable) disposable.Dispose();
            SetProperty(ref _currentViewModel, value);
        }
    }

    /// <summary>
    /// Mở form đăng nhập và xóa session cũ.
    /// </summary>
    private void ShowLogin()
    {
        _currentSession = null;
        CurrentViewModel = new LoginViewModel(_authService, HandleAuthenticated, ShowRegister);
    }

    /// <summary>
    /// Mở form đăng ký.
    /// </summary>
    private void ShowRegister() =>
        CurrentViewModel = new RegisterViewModel(_authService, HandleAuthenticated, ShowLogin);

    /// <summary>
    /// Lưu session và chọn trang Admin hoặc Student dựa trên Role.
    /// </summary>
    private void HandleAuthenticated(UserSession session)
    {
        _currentSession = session;
        if (session.Role == "Admin")
        {
            CurrentViewModel = new AdminViewModel(_adminService, session, ShowLogin);
        }
        else
        {
            ShowHome();
        }
    }

    /// <summary>
    /// Mở trang chủ học viên nếu session vẫn còn hợp lệ.
    /// </summary>
    private void ShowHome()
    {
        if (_currentSession is null) return;
        CurrentViewModel = new HomeViewModel(_examService, _currentSession, ShowExam, ShowReview, ShowLogin);
    }

    /// <summary>
    /// Tải đề và mở màn hình làm bài; async void phù hợp vì đây là callback điều hướng UI.
    /// </summary>
    private async void ShowExam(int examId)
    {
        if (_currentSession is null) return;
        try
        {
            var result = await _examService.StartExamAsync(examId);
            if (!result.IsSuccess || result.Data is null)
            {
                MessageBox.Show(result.Message, "Không thể mở đề", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CurrentViewModel = new ExamTakingViewModel(
                _examService, _currentSession, result.Data, ShowReview, ShowHome);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải đề thi.\n\nChi tiết: {ex.Message}", "Có lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Tải chi tiết một lần thi và mở màn hình xem đáp án.
    /// </summary>
    private async void ShowReview(int attemptId)
    {
        if (_currentSession is null) return;
        try
        {
            var result = await _examService.GetReviewAsync(attemptId, _currentSession.Id);
            if (!result.IsSuccess || result.Data is null)
            {
                MessageBox.Show(result.Message, "Không thể xem bài", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CurrentViewModel = new ReviewViewModel(result.Data, ShowHome, ShowExam);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải lịch sử.\n\nChi tiết: {ex.Message}", "Có lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
