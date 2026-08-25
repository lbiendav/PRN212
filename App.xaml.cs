using ExamManagementSystem.Business.Security;
using ExamManagementSystem.Business.Services;
using ExamManagementSystem.DataAccess;
using ExamManagementSystem.DataAccess.Repositories;
using ExamManagementSystem.Presentation.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace ExamManagementSystem;

/// <summary>
/// Điểm khởi động ứng dụng và nơi ghép các tầng lại với nhau (Composition Root).
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Đọc cấu hình, tạo dependency rồi hiển thị cửa sổ chính.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // appsettings.json nằm cạnh file .exe nhờ cấu hình trong csproj.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Nếu thiếu connection string, dừng sớm với thông báo dễ hiểu.
        var connectionString = configuration.GetConnectionString("ExamDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            MessageBox.Show("Không tìm thấy ConnectionStrings:ExamDatabase trong appsettings.json.",
                "Thiếu cấu hình", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        // Tạo tầng DataAccess: factory và các repository.
        var contextFactory = new ExamDbContextFactory(connectionString);
        IUserRepository userRepository = new UserRepository(contextFactory);
        IExamRepository examRepository = new ExamRepository(contextFactory);
        IAttemptRepository attemptRepository = new AttemptRepository(contextFactory);

        // Tạo tầng Business: security và các service nghiệp vụ.
        var passwordHasher = new PasswordHasher();
        var authService = new AuthService(userRepository, passwordHasher);
        var examService = new ExamService(examRepository, attemptRepository);
        var adminService = new AdminService(userRepository, examRepository, passwordHasher);

        // Tạo tầng Presentation và gán MainViewModel làm DataContext cho cửa sổ.
        var mainViewModel = new MainViewModel(authService, examService, adminService);
        var mainWindow = new MainWindow { DataContext = mainViewModel };
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}
