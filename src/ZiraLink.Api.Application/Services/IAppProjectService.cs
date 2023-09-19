using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZiraLink.Domain.Enums;
using ZiraLink.Domain;

namespace ZiraLink.Api.Application.Services
{
    public interface IAppProjectService
    {
        Task<List<AppProject>> GetAsync(long customerId, CancellationToken cancellationToken);
        Task<List<AppProject>> GetAllAsync(CancellationToken cancellationToken);
        Task<AppProject> GetByIdAsync(long id, long customerId, CancellationToken cancellationToken);
        Task<long> CreateAsync(long customerId, string title, Guid? appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state, CancellationToken cancellationToken);
        Task DeleteAsync(long customerId, long id, CancellationToken cancellationToken);
        Task PatchAsync(long id, long customerId, string title, Guid? appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state, CancellationToken cancellationToken);
    }
}
