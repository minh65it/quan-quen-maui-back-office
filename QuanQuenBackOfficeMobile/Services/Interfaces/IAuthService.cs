using System.Threading.Tasks;
using QuanQuenBackOfficeMobile.Models;

namespace QuanQuenBackOfficeMobile.Services.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Đăng nhập với email và password
    /// </summary>
    /// <param name="email">Email đăng nhập</param>
    /// <param name="password">Mật khẩu</param>
    /// <returns>Kết quả đăng nhập chứa token nếu thành công</returns>
    Task<LoginResponse> LoginAsync(string email, string password);

    /// <summary>
    /// Đăng xuất khỏi hệ thống
    /// </summary>
    /// <returns>True nếu đăng xuất thành công</returns>
    Task<bool> LogoutAsync();
} 