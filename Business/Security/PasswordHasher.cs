using System.Security.Cryptography;

namespace ExamManagementSystem.Business.Security;

/// <summary>
/// Băm và kiểm tra mật khẩu bằng PBKDF2, giúp không lưu mật khẩu dạng văn bản.
/// </summary>
public class PasswordHasher
{
    // Số vòng lặp càng lớn thì việc dò mật khẩu càng tốn thời gian.
    private const int Iterations = 100_000;

    /// <summary>
    /// Tạo salt ngẫu nhiên và trả chuỗi theo mẫu: số vòng lặp.salt.hash.
    /// </summary>
    public string Hash(string password)
    {
        // Salt khiến hai người dùng cùng mật khẩu vẫn có hash khác nhau.
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            32);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Băm mật khẩu nhập vào với salt cũ rồi so sánh theo thời gian cố định.
    /// </summary>
    public bool Verify(string password, string storedHash)
    {
        // Dữ liệu không đúng định dạng được coi là mật khẩu không hợp lệ.
        var parts = storedHash.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        try
        {
            // Tạo lại hash từ mật khẩu người dùng vừa nhập.
            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            // Hash lỗi trong database không được làm ứng dụng bị dừng.
            return false;
        }
    }
}
