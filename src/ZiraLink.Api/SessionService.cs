using System.Text.Json;
using IdentityModel.Client;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Framework;
using ZiraLink.Api.Models;
using ZiraLink.Domain;

namespace ZiraLink.Api
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomerService _customerService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public SessionService(IHttpContextAccessor httpContextAccessor, ICustomerService customerService, ITokenService tokenService, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _customerService = customerService;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<Customer> GetCurrentCustomer(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(claim => claim.Type == "sub");
            var customer = await _customerService.GetCustomerByExternalIdAsync(userId.Value, cancellationToken);

            return customer;
        }

        public async Task<ProfileViewModel> GetCurrentCustomerProfile(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(claim => claim.Type == "sub");

            var token = await _tokenService.GetTokenBySubAsync(userId.Value);
            var baseUri = new Uri(_configuration["ZIRALINK_URL_IDS"]!);
            var uri = new Uri(baseUri, "connect/userinfo");
            var userInfoRequest = new UserInfoRequest
            {
                Address = uri.ToString(),
                Token = token
            };

            var client = new HttpClient();
            var userInfoResponse = await client.GetUserInfoAsync(userInfoRequest);
            var result = await userInfoResponse.HttpResponse.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ProfileViewModel>(result);
        }
    }
}
