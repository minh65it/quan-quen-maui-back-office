using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.Services.Interfaces;

public interface ITokenService
{
    Task<bool> IsTokenValidAsync();
} 