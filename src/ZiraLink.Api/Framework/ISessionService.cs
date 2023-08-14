using ZiraLink.Domain;

namespace ZiraLink.Api.Framework
{
    public interface ISessionService
    {
        Task<Customer> GetCurrentCustomer(CancellationToken cancellationToken);
    }
}
