using System.Windows.Input;

namespace ExamManagementSystem.Presentation.MVVM;

/// <summary>
/// Biến một Action thành ICommand để Button gọi logic trong ViewModel.
/// </summary>
public class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) : ICommand
{
    // Hàm được chạy khi người dùng bấm nút.
    private readonly Action<object?> _execute = execute;

    // Hàm tùy chọn quyết định nút có được phép bấm hay không.
    private readonly Predicate<object?>? _canExecute = canExecute;

    // WPF gọi sự kiện này khi trạng thái CanExecute cần được tính lại.
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    // Nếu không truyền điều kiện thì lệnh luôn được phép chạy.
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    // Chuyển tiếp tham số từ Button sang Action.
    public void Execute(object? parameter) => _execute(parameter);
}
