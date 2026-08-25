using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ExamManagementSystem.Business.DTOs;
using ExamManagementSystem.Business.Services;
using ExamManagementSystem.Presentation.MVVM;

namespace ExamManagementSystem.Presentation.ViewModels;

/// <summary>
/// ViewModel trang quản trị, gom CRUD/Search/Filter tài khoản và đề thi.
/// </summary>
public class AdminViewModel : ViewModelBase
{
    // Service và phiên admin hiện tại dùng cho các quy tắc nghiệp vụ.
    private readonly AdminService _adminService;
    private readonly UserSession _session;

    // Trạng thái tìm kiếm người dùng và dữ liệu form người dùng.
    private string _userKeyword = string.Empty;
    private string _userRoleFilter = "Tất cả";
    private string _userStatusFilter = "Tất cả";
    private int _editingUserId;
    private string _editUsername = string.Empty;
    private string _editFullName = string.Empty;
    private string _editEmail = string.Empty;
    private string _editUserRole = "Student";
    private string _editUserPassword = string.Empty;
    private bool _editUserIsActive = true;
    private string _userMessage = string.Empty;

    // Trạng thái tìm kiếm đề và dữ liệu form đề thi.
    private string _examKeyword = string.Empty;
    private string _examSubjectFilter = string.Empty;
    private string _examStatusFilter = "Tất cả";
    private int _editingExamId;
    private string _editExamTitle = string.Empty;
    private string _editExamSubject = string.Empty;
    private string _editExamDescription = string.Empty;
    private int _editExamDuration = 15;
    private bool _editExamIsActive;
    private string _examMessage = string.Empty;

    /// <summary>
    /// Khởi tạo toàn bộ command của hai tab và bắt đầu tải dữ liệu.
    /// </summary>
    public AdminViewModel(AdminService adminService, UserSession session, Action logout)
    {
        _adminService = adminService;
        _session = session;
        AdminName = $"Quản trị viên: {session.FullName}";

        SearchUsersCommand = new AsyncRelayCommand(_ => LoadUsersAsync());
        NewUserCommand = new RelayCommand(_ => ClearUserForm());
        EditUserCommand = new AsyncRelayCommand(EditUserAsync);
        SaveUserCommand = new AsyncRelayCommand(_ => SaveUserAsync());
        DeleteUserCommand = new AsyncRelayCommand(DeleteUserAsync);

        SearchExamsCommand = new AsyncRelayCommand(_ => LoadExamsAsync());
        NewExamCommand = new RelayCommand(_ => ClearExamForm());
        EditExamCommand = new AsyncRelayCommand(EditExamAsync);
        SaveExamCommand = new AsyncRelayCommand(_ => SaveExamAsync());
        DeleteExamCommand = new AsyncRelayCommand(DeleteExamAsync);
        LogoutCommand = new RelayCommand(_ => logout());
        InitializeCommand = new AsyncRelayCommand(_ => LoadAllAsync());

        // Tải lần đầu qua command để lỗi kết nối được chặn và hiển thị thân thiện.
        InitializeCommand.Execute(null);
    }

    // Danh sách và lựa chọn cố định dùng cho ComboBox.
    public ObservableCollection<UserListItemDto> Users { get; } = [];
    public ObservableCollection<ExamListItemDto> Exams { get; } = [];
    public IReadOnlyList<string> Roles { get; } = ["Tất cả", "Student", "Admin"];
    public IReadOnlyList<string> EditRoles { get; } = ["Student", "Admin"];
    public IReadOnlyList<string> Statuses { get; } = ["Tất cả", "Hoạt động", "Đã khóa/ẩn"];
    public string AdminName { get; }

    // Thuộc tính Binding của vùng tìm kiếm/lọc và form người dùng.
    public string UserKeyword { get => _userKeyword; set => SetProperty(ref _userKeyword, value); }
    public string UserRoleFilter { get => _userRoleFilter; set => SetProperty(ref _userRoleFilter, value); }
    public string UserStatusFilter { get => _userStatusFilter; set => SetProperty(ref _userStatusFilter, value); }
    public int EditingUserId { get => _editingUserId; set => SetProperty(ref _editingUserId, value); }
    public string EditUsername { get => _editUsername; set => SetProperty(ref _editUsername, value); }
    public string EditFullName { get => _editFullName; set => SetProperty(ref _editFullName, value); }
    public string EditEmail { get => _editEmail; set => SetProperty(ref _editEmail, value); }
    public string EditUserRole { get => _editUserRole; set => SetProperty(ref _editUserRole, value); }
    public string EditUserPassword { get => _editUserPassword; set => SetProperty(ref _editUserPassword, value); }
    public bool EditUserIsActive { get => _editUserIsActive; set => SetProperty(ref _editUserIsActive, value); }
    public string UserMessage { get => _userMessage; set => SetProperty(ref _userMessage, value); }
    public string UserFormTitle => EditingUserId == 0 ? "Thêm tài khoản" : $"Sửa tài khoản #{EditingUserId}";

    // Thuộc tính Binding của vùng tìm kiếm/lọc và form đề thi.
    public string ExamKeyword { get => _examKeyword; set => SetProperty(ref _examKeyword, value); }
    public string ExamSubjectFilter { get => _examSubjectFilter; set => SetProperty(ref _examSubjectFilter, value); }
    public string ExamStatusFilter { get => _examStatusFilter; set => SetProperty(ref _examStatusFilter, value); }
    public int EditingExamId { get => _editingExamId; set => SetProperty(ref _editingExamId, value); }
    public string EditExamTitle { get => _editExamTitle; set => SetProperty(ref _editExamTitle, value); }
    public string EditExamSubject { get => _editExamSubject; set => SetProperty(ref _editExamSubject, value); }
    public string EditExamDescription { get => _editExamDescription; set => SetProperty(ref _editExamDescription, value); }
    public int EditExamDuration { get => _editExamDuration; set => SetProperty(ref _editExamDuration, value); }
    public bool EditExamIsActive { get => _editExamIsActive; set => SetProperty(ref _editExamIsActive, value); }
    public string ExamMessage { get => _examMessage; set => SetProperty(ref _examMessage, value); }
    public string ExamFormTitle => EditingExamId == 0 ? "Thêm đề thi" : $"Sửa đề thi #{EditingExamId}";

    // Command của tab tài khoản.
    public ICommand SearchUsersCommand { get; }
    public ICommand NewUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand SaveUserCommand { get; }
    public ICommand DeleteUserCommand { get; }

    // Command của tab đề thi và command dùng chung.
    public ICommand SearchExamsCommand { get; }
    public ICommand NewExamCommand { get; }
    public ICommand EditExamCommand { get; }
    public ICommand SaveExamCommand { get; }
    public ICommand DeleteExamCommand { get; }
    public ICommand LogoutCommand { get; }

    // Command nội bộ chỉ chạy một lần khi ViewModel vừa được tạo.
    private ICommand InitializeCommand { get; }

    /// <summary>
    /// Tải đồng thời hai bảng quản trị.
    /// </summary>
    private async Task LoadAllAsync() => await Task.WhenAll(LoadUsersAsync(), LoadExamsAsync());

    /// <summary>
    /// Chuyển giá trị bộ lọc hiển thị sang tham số Business và tải Users.
    /// </summary>
    private async Task LoadUsersAsync()
    {
        var role = UserRoleFilter == "Tất cả" ? string.Empty : UserRoleFilter;
        var status = ParseStatus(UserStatusFilter);
        ReplaceItems(Users, await _adminService.SearchUsersAsync(UserKeyword, role, status));
    }

    /// <summary>
    /// Đổ dữ liệu tài khoản được chọn vào form sửa.
    /// </summary>
    private async Task EditUserAsync(object? parameter)
    {
        if (parameter is not UserListItemDto item) return;
        var dto = await _adminService.GetUserAsync(item.Id);
        if (dto is null) return;

        EditingUserId = dto.Id;
        EditUsername = dto.Username;
        EditFullName = dto.FullName;
        EditEmail = dto.Email;
        EditUserRole = dto.Role;
        EditUserPassword = string.Empty;
        EditUserIsActive = dto.IsActive;
        UserMessage = "Để trống mật khẩu nếu muốn giữ mật khẩu cũ.";
        OnPropertyChanged(nameof(UserFormTitle));
    }

    /// <summary>
    /// Gom dữ liệu form thành DTO rồi gọi nghiệp vụ thêm/sửa.
    /// </summary>
    private async Task SaveUserAsync()
    {
        var result = await _adminService.SaveUserAsync(new UserEditDto
        {
            Id = EditingUserId,
            Username = EditUsername,
            FullName = EditFullName,
            Email = EditEmail,
            Role = EditUserRole,
            Password = EditUserPassword,
            IsActive = EditUserIsActive
        });
        UserMessage = result.Message;
        if (!result.IsSuccess) return;

        ClearUserForm();
        UserMessage = result.Message;
        await LoadUsersAsync();
    }

    /// <summary>
    /// Hỏi xác nhận trước khi yêu cầu Business xóa tài khoản.
    /// </summary>
    private async Task DeleteUserAsync(object? parameter)
    {
        if (parameter is not UserListItemDto item) return;
        if (MessageBox.Show($"Xóa tài khoản '{item.Username}'?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

        var result = await _adminService.DeleteUserAsync(item.Id, _session.Id);
        UserMessage = result.Message;
        if (result.IsSuccess) await LoadUsersAsync();
    }

    /// <summary>
    /// Đặt form người dùng về trạng thái thêm mới.
    /// </summary>
    private void ClearUserForm()
    {
        EditingUserId = 0;
        EditUsername = string.Empty;
        EditFullName = string.Empty;
        EditEmail = string.Empty;
        EditUserRole = "Student";
        EditUserPassword = string.Empty;
        EditUserIsActive = true;
        UserMessage = string.Empty;
        OnPropertyChanged(nameof(UserFormTitle));
    }

    /// <summary>
    /// Tải danh sách đề theo từ khóa, môn học và trạng thái.
    /// </summary>
    private async Task LoadExamsAsync()
    {
        ReplaceItems(Exams, await _adminService.SearchExamsAsync(
            ExamKeyword, ExamSubjectFilter.Trim(), ParseStatus(ExamStatusFilter)));
    }

    /// <summary>
    /// Đổ dữ liệu đề được chọn vào form sửa.
    /// </summary>
    private async Task EditExamAsync(object? parameter)
    {
        if (parameter is not ExamListItemDto item) return;
        var dto = await _adminService.GetExamAsync(item.Id);
        if (dto is null) return;

        EditingExamId = dto.Id;
        EditExamTitle = dto.Title;
        EditExamSubject = dto.Subject;
        EditExamDescription = dto.Description;
        EditExamDuration = dto.DurationMinutes;
        EditExamIsActive = dto.IsActive;
        ExamMessage = "Đang sửa đề đã chọn.";
        OnPropertyChanged(nameof(ExamFormTitle));
    }

    /// <summary>
    /// Gom form đề thi thành DTO rồi gọi nghiệp vụ thêm/sửa.
    /// </summary>
    private async Task SaveExamAsync()
    {
        var result = await _adminService.SaveExamAsync(new ExamEditDto
        {
            Id = EditingExamId,
            Title = EditExamTitle,
            Subject = EditExamSubject,
            Description = EditExamDescription,
            DurationMinutes = EditExamDuration,
            IsActive = EditExamIsActive
        });
        ExamMessage = result.Message;
        if (!result.IsSuccess) return;

        ClearExamForm();
        ExamMessage = result.Message;
        await LoadExamsAsync();
    }

    /// <summary>
    /// Hỏi xác nhận trước khi xóa đề chưa có lịch sử.
    /// </summary>
    private async Task DeleteExamAsync(object? parameter)
    {
        if (parameter is not ExamListItemDto item) return;
        if (MessageBox.Show($"Xóa đề '{item.Title}'?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

        var result = await _adminService.DeleteExamAsync(item.Id);
        ExamMessage = result.Message;
        if (result.IsSuccess) await LoadExamsAsync();
    }

    /// <summary>
    /// Đặt form đề về trạng thái thêm mới; đề mới mặc định bị ẩn vì chưa có câu hỏi.
    /// </summary>
    private void ClearExamForm()
    {
        EditingExamId = 0;
        EditExamTitle = string.Empty;
        EditExamSubject = string.Empty;
        EditExamDescription = string.Empty;
        EditExamDuration = 15;
        EditExamIsActive = false;
        ExamMessage = string.Empty;
        OnPropertyChanged(nameof(ExamFormTitle));
    }

    /// <summary>
    /// Chuyển trạng thái tiếng Việt thành bool?; null tương ứng "Tất cả".
    /// </summary>
    private static bool? ParseStatus(string value) => value switch
    {
        "Hoạt động" => true,
        "Đã khóa/ẩn" => false,
        _ => null
    };

    /// <summary>
    /// Thay dữ liệu trong ObservableCollection hiện có.
    /// </summary>
    private static void ReplaceItems<T>(ObservableCollection<T> target, IEnumerable<T> values)
    {
        target.Clear();
        foreach (var value in values) target.Add(value);
    }
}
