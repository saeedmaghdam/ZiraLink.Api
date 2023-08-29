using System.IdentityModel.Tokens.Jwt;

namespace ZiraLink.Api
{
    public interface ITokenService
    {
        JwtSecurityToken ParseToken(string token);
        Task<string> GenerateToken(string sub, string name, string family);
        Task SetTokenPSubAsync(string tokenp, string sub);
        Task<string?> GetTokenPSubAsync(string tokenp);
        Task SetSubTokenAsync(string sub, string token);
        Task<string?> GetSunTokenAsync(string sub);
        Task SetSubTokenPAsync(string sub, string tokenp);
        Task<string?> GetSubTokenPAsync(string sub);
        Task SetTokenPTokenAsync(string tokenp, string token);
        Task<string?> GetTokenPTokenAsync(string tokenp);
        Task SetSubIdTokenAsync(string sub, string id_token);
        Task<string?> GetSubIdTokenAsync(string sub);
        Task SetTokenPRefreshTokenAsync(string sub, string refreshToken);
        Task<string?> GetTokenPRefreshTokenAsync(string sub);
    }
}
