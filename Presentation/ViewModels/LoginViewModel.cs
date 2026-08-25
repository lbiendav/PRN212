using System.Windows.Input;
using ExamManagementSystem.Business.DTOs;
using ExamManagementSystem.Business.Services;
using ExamManagementSystem.Presentation.MVVM;

namespace ExamManagementSystem.Presentation.ViewModels;

/// <summary>
/// ViewModel của màn hình đăng nhập.
/// </summary>
public class LoginViewModel : ViewModelBase
{
    // Service xử lý xác thực và callback giúp MainViewModel điều hướng.
    private readonly AuthService _authService;
    private readonly Action<UserSession> _onLoggedIn;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _message = string.Empty;

    /// <summary>
    /// Khởi tạo lệnh đăng nhập và lệnh chuyển sang đăng ký.
    /// </summary>
    public LoginViewModel(AuthService authService, Action<UserSession> onLoggedIn, Action showRegister)
    {
        _authService = authService;
        _onLoggedIn = onLoggedIn;
        LoginCommand = new AsyncRelayCommand(_ => LoginAsync());
        ShowRegisterCommand = new RelayCommand(_ => showRegister());
    }

    // Các thuộc tính được Binding với ô nhập trên LoginView.
    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }

    // Hai lệnh được Button gọi thay cho code-behind.
    public ICommand LoginCommand { get; }
    public ICommand ShowRegisterCommand { get; }

    /// <summary>
    /// Gọi tầng Business và chuyển trang nếu tài khoản hợp lệ.
    /// </summary>
    private async Task LoginAsync()
    {
        Message = string.Empty;
        var result = await _authService.LoginAsync(Username, Password);
        if (!result.IsSuccess || result.Data is null)
        {
            Message = result.Message;
            return;
        }

        // Xóa mật khẩu khỏi bộ nhớ ViewModel ngay sau khi đăng nhập.
        Password = string.Empty;
        _onLoggedIn(result.Data);
    }
}
