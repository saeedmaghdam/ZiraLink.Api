using ZiraLink.Domain;

namespace ZiraLink.Api.Application
{
    public interface ICustomerService
    {
        Task<Customer> GetCustomerByExternalIdAsync(string externalId, CancellationToken cancellationToken);
        Task<Guid> CreateLocallyAsync(string externalId, string username, string email, string name, string family, CancellationToken cancellationToken);
        Task<Guid> CreateAsync(string username, string password, string email, string name, string family, CancellationToken cancellationToken);
        Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
        Task UpdateProfileAsync(string userId, string name, string family, CancellationToken cancellationToken);
    }
}
