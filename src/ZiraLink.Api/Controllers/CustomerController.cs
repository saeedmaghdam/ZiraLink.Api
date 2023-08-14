using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiraLink.Api.Framework;
using ZiraLink.Api.Models;

namespace ZiraLink.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    public class CustomerController
    {

        private readonly ISessionService _sessionService;

        public CustomerController(ISessionService sessionService) => _sessionService = sessionService;

        [HttpGet("Profile")]
        public async Task<ApiResponse<ProfileViewModel>> GetProfileAsync(CancellationToken cancellationToken)
        {
            var result = await _sessionService.GetCurrentCustomerProfile(cancellationToken);

            return ApiResponse<ProfileViewModel>.CreateSuccessResponse(result);
        }
    }
}
