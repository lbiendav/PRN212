using System.Windows.Input;
using ExamManagementSystem.Business.DTOs;
using ExamManagementSystem.Business.Services;
using ExamManagementSystem.Presentation.MVVM;

namespace ExamManagementSystem.Presentation.ViewModels;

/// <summary>
/// ViewModel của màn hình đăng ký tài khoản học viên.
/// </summary>
public class RegisterViewModel : ViewModelBase
{
    // Dependency và callback phục vụ đăng ký/điều hướng.
    private readonly AuthService _authService;
    private readonly Action<UserSession> _onRegistered;
    private string _username = string.Empty;
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _message = string.Empty;

    /// <summary>
    /// Khởi tạo các command cho form đăng ký.
    /// </summary>
    public RegisterViewModel(AuthService authService, Action<UserSession> onRegistered, Action showLogin)
    {
        _authService = authService;
        _onRegistered = onRegistered;
        RegisterCommand = new AsyncRelayCommand(_ => RegisterAsync());
        ShowLoginCommand = new RelayCommand(_ => showLogin());
    }

    // Thuộc tính Binding TwoWay với các ô nhập liệu.
    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }

    // Lệnh tạo tài khoản và trở lại đăng nhập.
    public ICommand RegisterCommand { get; }
    public ICommand ShowLoginCommand { get; }

    /// <summary>
    /// Gửi form sang AuthService và tự đăng nhập khi đăng ký thành công.
    /// </summary>
    private async Task RegisterAsync()
    {
        Message = string.Empty;
        var result = await _authService.RegisterAsync(Username, FullName, Email, Password, ConfirmPassword);
        if (!result.IsSuccess || result.Data is null)
        {
            Message = result.Message;
            return;
        }

        // Dọn mật khẩu rồi chuyển tới trang chủ học viên.
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        _onRegistered(result.Data);
    }
}
