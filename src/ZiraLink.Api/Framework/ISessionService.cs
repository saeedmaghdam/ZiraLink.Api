using ZiraLink.Api.Models;
using ZiraLink.Domain;

namespace ZiraLink.Api.Framework
{
    public interface ISessionService
    {
        Task<Customer> GetCurrentCustomer(CancellationToken cancellationToken);
        Task<ProfileViewModel> GetCurrentCustomerProfile(CancellationToken cancellationToken);
    }
}
