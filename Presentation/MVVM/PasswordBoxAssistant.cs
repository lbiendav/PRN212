using System.Windows;
using System.Windows.Controls;

namespace ExamManagementSystem.Presentation.MVVM;

/// <summary>
/// Attached Property giúp PasswordBox trao đổi dữ liệu với ViewModel vì WPF không hỗ trợ Binding Password mặc định.
/// </summary>
public static class PasswordBoxAssistant
{
    // Cờ nội bộ tránh vòng lặp cập nhật Password vô hạn.
    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxAssistant));

    // Thuộc tính Password mà XAML có thể Binding TwoWay.
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.RegisterAttached(
            "Password",
            typeof(string),
            typeof(PasswordBoxAssistant),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));

    // Cờ bật/tắt behavior theo dõi sự kiện PasswordChanged.
    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached(
            "Attach",
            typeof(bool),
            typeof(PasswordBoxAssistant),
            new PropertyMetadata(false, OnAttachChanged));

    // Các hàm Get/Set là quy ước bắt buộc của Attached Property trong WPF.
    public static string GetPassword(DependencyObject obj) => (string)obj.GetValue(PasswordProperty);
    public static void SetPassword(DependencyObject obj, string value) => obj.SetValue(PasswordProperty, value);
    public static bool GetAttach(DependencyObject obj) => (bool)obj.GetValue(AttachProperty);
    public static void SetAttach(DependencyObject obj, bool value) => obj.SetValue(AttachProperty, value);

    /// <summary>
    /// Đăng ký hoặc hủy sự kiện khi giá trị Attach thay đổi.
    /// </summary>
    private static void OnAttachChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
        {
            return;
        }

        passwordBox.PasswordChanged -= HandlePasswordChanged;
        if ((bool)e.NewValue)
        {
            passwordBox.PasswordChanged += HandlePasswordChanged;
        }
    }

    /// <summary>
    /// Cập nhật PasswordBox khi ViewModel thay đổi thuộc tính Password.
    /// </summary>
    private static void OnPasswordChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox || (bool)passwordBox.GetValue(IsUpdatingProperty))
        {
            return;
        }

        passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Đẩy mật khẩu vừa nhập từ control về ViewModel.
    /// </summary>
    private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
    {
        var passwordBox = (PasswordBox)sender;
        passwordBox.SetValue(IsUpdatingProperty, true);
        SetPassword(passwordBox, passwordBox.Password);
        passwordBox.SetValue(IsUpdatingProperty, false);
    }
}
