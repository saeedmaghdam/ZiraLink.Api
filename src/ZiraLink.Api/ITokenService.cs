using System.IdentityModel.Tokens.Jwt;

namespace ZiraLink.Api
{
    public interface ITokenService
    {
        JwtSecurityToken ParseToken(string token);
        Task<string> GenerateToken(string sub, string name, string family);
        Task SetTokenPSubAsync(string tokenp, string sub);
        Task<string?> GetSubByTokenPAsync(string tokenp);
        Task SetSubTokenAsync(string sub, string token);
        Task<string?> GetTokenBySubAsync(string sub);
        Task SetSubTokenPAsync(string sub, string tokenp);
        Task<string?> GetTokenPBySubAsync(string sub);
        Task SetTokenPTokenAsync(string tokenp, string token);
        Task<string?> GetTokenByTokenP(string tokenp);
    }
}
