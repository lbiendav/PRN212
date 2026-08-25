using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExamManagementSystem.Presentation.MVVM;

/// <summary>
/// Lớp cha của mọi ViewModel, hỗ trợ thông báo khi thuộc tính thay đổi.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    // WPF lắng nghe sự kiện này để tự cập nhật giao diện khi dữ liệu thay đổi.
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gán giá trị mới và phát sự kiện nếu giá trị thật sự thay đổi.
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    /// <summary>
    /// Chủ động báo một thuộc tính tính toán đã thay đổi.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
