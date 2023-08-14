using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ZiraLink.Api
{
    public class TokenService : ITokenService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        private readonly IDatabase _db;

        public TokenService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _db = _connectionMultiplexer.GetDatabase(10);
        }

        public JwtSecurityToken ParseToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);

            return jwtSecurityToken;
        }

        public async Task<string> GenerateToken(string sub, string name, string family)
        {
            // check if the user has the required permissions

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, $"{name} {family}"),
                    new Claim(ClaimTypes.GivenName, name),
                    new Claim(ClaimTypes.Surname, family),
                    new Claim("user_id", sub)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII
                    .GetBytes("576537ae-b3da-4393-9233-68190b1646e3")), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        public async Task SetTokenPSubAsync(string tokenp, string sub)
        {
            await _db.StringSetAsync($"tokenpsub:{tokenp}", sub);
        }

        public async Task<string?> GetSubByTokenPAsync(string tokenp)
        {
            return await _db.StringGetAsync($"tokenpsub:{tokenp}");
        }

        public async Task SetSubTokenAsync(string sub, string token)
        {
            await _db.StringSetAsync($"token:{sub}", token);
        }

        public async Task<string?> GetTokenBySubAsync(string sub)
        {
            return await _db.StringGetAsync($"token:{sub}");
        }

        public async Task SetSubTokenPAsync(string sub, string tokenp)
        {
            await _db.StringSetAsync($"tokenp:{sub}", tokenp);
        }

        public async Task<string?> GetTokenPBySubAsync(string sub)
        {
            return await _db.StringGetAsync($"tokenp:{sub}");
        }

        public async Task SetTokenPTokenAsync(string tokenp, string token)
        {
            await _db.StringSetAsync($"tokenptoken:{tokenp}", token);
        }

        public async Task<string?> GetTokenByTokenP(string tokenp)
        {
            return await _db.StringGetAsync($"tokenptoken:{tokenp}");
        }

        public async Task SetSubIdTokenAsync(string sub, string id_token)
        {
            await _db.StringSetAsync($"sub:{sub}", id_token);
        }

        public async Task<string?> GetIdTokenSubAsync(string sub)
        {
            return await _db.StringGetAsync($"sub:{sub}");
        }
    }
}
