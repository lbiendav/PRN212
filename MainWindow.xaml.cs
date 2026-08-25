using System.Windows;

namespace ExamManagementSystem;

/// <summary>
/// Code-behind chỉ khởi tạo View; mọi xử lý giao diện nằm trong MainViewModel.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Khởi tạo các control đã khai báo trong XAML.
    /// </summary>
    public MainWindow() => InitializeComponent();
}
