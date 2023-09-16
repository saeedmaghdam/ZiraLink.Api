using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.Application.Services
{
    public interface IProjectService
    {
        Task<List<Project>> GetAsync(long customerId, CancellationToken cancellationToken);
        Task<List<Project>> GetAllAsync(CancellationToken cancellationToken);
        Task<Project> GetByIdAsync(long id, long customerId, CancellationToken cancellationToken);
        Task<Guid> CreateAsync(long id, string title, DomainType domainType, string domain, string internalUrl, ProjectState state, CancellationToken cancellationToken);
        Task DeleteAsync(long customerId, long id, CancellationToken cancellationToken);
        Task PatchAsync(long id, long customerId, string title, DomainType domainType, string domain, string internalUrl, ProjectState state, CancellationToken cancellationToken);
    }
}
