namespace ExamManagementSystem.Business.DTOs;

/// <summary>
/// Bao bọc kết quả nghiệp vụ để ViewModel nhận cả dữ liệu và thông báo lỗi.
/// </summary>
public class ServiceResult<T>
{
    // Cho biết thao tác thành công hay thất bại.
    public bool IsSuccess { get; init; }

    // Thông báo thân thiện để hiển thị cho người dùng.
    public string Message { get; init; } = string.Empty;

    // Dữ liệu trả về; có thể null khi thao tác thất bại.
    public T? Data { get; init; }

    // Hàm hỗ trợ tạo nhanh kết quả thành công.
    public static ServiceResult<T> Success(T data, string message = "") =>
        new() { IsSuccess = true, Data = data, Message = message };

    // Hàm hỗ trợ tạo nhanh kết quả thất bại.
    public static ServiceResult<T> Failure(string message) =>
        new() { IsSuccess = false, Message = message };
}
