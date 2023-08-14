using ZiraLink.Domain;

namespace ZiraLink.Api.Application
{
    public interface ICustomerService
    {
        Task<Customer> GetCustomerByExternalIdAsync(string externalId, CancellationToken cancellationToken);
    }
}
