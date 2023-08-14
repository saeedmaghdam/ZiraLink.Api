using StackExchange.Redis;
using ZiraLink.Api.Application;
using ZiraLink.Api.Framework;
using ZiraLink.Domain;

namespace ZiraLink.Api
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomerService _customerService;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public SessionService(IHttpContextAccessor httpContextAccessor, ICustomerService customerService, IConnectionMultiplexer connectionMultiplexer)
        {
            _httpContextAccessor = httpContextAccessor;
            _customerService = customerService;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<Customer> GetCurrentCustomer(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(claim => claim.Type == "sub");
            var customer = await _customerService.GetCustomerByExternalIdAsync(userId.Value, cancellationToken);

            return customer;
        }
    }
}
