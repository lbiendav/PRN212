using System.Windows;
using System.Windows.Input;

namespace ExamManagementSystem.Presentation.MVVM;

/// <summary>
/// ICommand dành cho hàm async, đồng thời chặn người dùng bấm liên tục.
/// </summary>
public class AsyncRelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null) : ICommand
{
    // Hàm bất đồng bộ cần thực hiện và điều kiện cho phép thực hiện.
    private readonly Func<object?, Task> _execute = execute;
    private readonly Predicate<object?>? _canExecute = canExecute;
    private bool _isExecuting;

    // Đăng ký với CommandManager để giao diện tự cập nhật trạng thái nút.
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    // Không cho chạy lần hai khi tác vụ cũ vẫn đang thực hiện.
    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

    /// <summary>
    /// Chạy tác vụ, hiển thị lỗi thân thiện và luôn mở khóa nút sau khi xong.
    /// </summary>
    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        try
        {
            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();
            await _execute(parameter);
        }
        catch (Exception ex)
        {
            // Lỗi kết nối/SQL được chặn tại Presentation để ứng dụng không bị đóng đột ngột.
            MessageBox.Show($"Không thể hoàn thành thao tác.\n\nChi tiết: {ex.Message}",
                "Có lỗi xảy ra", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isExecuting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
