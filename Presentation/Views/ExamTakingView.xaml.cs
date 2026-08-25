using System.Windows.Controls;

namespace ExamManagementSystem.Presentation.Views;

/// <summary>
/// Code-behind không chứa logic; timer và câu trả lời được quản lý trong ExamTakingViewModel.
/// </summary>
public partial class ExamTakingView : UserControl
{
    // Đọc và dựng cây giao diện từ XAML.
    public ExamTakingView() => InitializeComponent();
}
