using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.Application
{
    public interface IProjectService
    {
        Task<Guid> CreateAsync(string customerExternalId, string title, DomainType domainType, string domain, string internalUrl, CancellationToken cancellationToken);
    }
}
