using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using ZiraLink.Api.Application.Framework;
using ZiraLink.Api.Framework;

namespace ZiraLink.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenService _tokenService;

        public UserController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ITokenService tokenService)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
        }

        [HttpPost("RefreshToken")]
        public async Task<ApiDefaultResponse> RefreshTokenAsync()
        {
            var client = new HttpClient();

            var tokenp = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            tokenp = tokenp.ToString().Replace("Bearer ", "");
            var sub = await _tokenService.GetTokenPSubAsync(tokenp);
            var refreshToken = await _tokenService.GetTokenPRefreshTokenAsync(tokenp);
            var baseUri = new Uri(_configuration["ZIRALINK_URL_IDS"]!);
            var uri = new Uri(baseUri, "connect/token");
            var response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = uri.ToString(),
                ClientId = "bff",
                ClientSecret = "secret",
                RefreshToken = refreshToken
            });
            var token = response.AccessToken;
            if (string.IsNullOrEmpty(token))
                return ApiDefaultResponse.CreateFailureResponse();

            await _tokenService.SetSubTokenAsync(sub, token);
            await _tokenService.SetTokenPTokenAsync(tokenp, token);

            return ApiDefaultResponse.CreateSuccessResponse();
        }
    }
}
