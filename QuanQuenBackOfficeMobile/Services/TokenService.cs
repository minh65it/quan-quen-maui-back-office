using Microsoft.Maui.Storage;
using System.IdentityModel.Tokens.Jwt;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Services
{
    public class TokenService : ITokenService
    {
        public async Task<bool> IsTokenValidAsync()
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var exp = jwt.ValidTo;
                return exp > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }
    }
} 