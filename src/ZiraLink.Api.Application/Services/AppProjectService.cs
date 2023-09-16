using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Api.Application.Tools;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.Application.Services
{
    public class AppProjectService : IAppProjectService
    {
        private readonly ILogger<ProjectService> _logger;
        private readonly AppDbContext _dbContext;
        private readonly IBus _bus;
        private readonly IHttpTools _httpTools;

        public AppProjectService(ILogger<ProjectService> logger, AppDbContext dbContext, IBus bus, IHttpTools httpTools)
        {
            _logger = logger;
            _dbContext = dbContext;
            _bus = bus;
            _httpTools = httpTools;
        }

        public async Task<List<AppProject>> GetAsync(long customerId, CancellationToken cancellationToken)
        {
            return await _dbContext.AppProjects.Include(x => x.Customer).AsNoTracking().Where(x => x.Customer.Id == customerId).ToListAsync(cancellationToken);
        }

        public async Task<List<AppProject>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.AppProjects.Include(x => x.Customer).AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<AppProject> GetByIdAsync(long id, long customerId, CancellationToken cancellationToken)
        {
            var project = await _dbContext.AppProjects.Include(x => x.Customer).AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.Customer.Id == customerId, cancellationToken);
            if (project == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), customerId) });

            return project;
        }

        public async Task<long> CreateAsync(long customerId, string title, Guid? appProjectViewId, AppProjectType appProjectType, int internalPort, RowState state, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));
            if (internalPort < 1 || internalPort > 65535)
                throw new ApplicationException("Port range is not valid");
            if (appProjectType == AppProjectType.UsePort && (!appProjectViewId.HasValue || appProjectViewId == Guid.Empty))
                throw new ArgumentNullException(nameof(appProjectViewId));

            var customer = await _dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == customerId, cancellationToken);
            if (customer == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), customerId) });

            //Guid appProjectViewIdGuid = string.IsNullOrWhiteSpace(appProjectViewId) ? Guid.Empty : Guid.Parse(appProjectViewId);
 
            if (appProjectType == AppProjectType.UsePort && !_dbContext.AppProjects.Any(x => x.ViewId == appProjectViewId))
                throw new NotFoundException(nameof(AppProject), new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>(nameof(AppProject.AppProjectViewId), appProjectViewId.Value) });

            var appProject = new AppProject
            {
                ViewId = Guid.NewGuid(),
                AppProjectViewId = appProjectViewId,
                CustomerId = customer.Id,
                Title = title,
                AppProjectType = appProjectType,
                InternalPort = internalPort,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                State = state
            };

            await _dbContext.AppProjects.AddAsync(appProject, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _bus.Publish("APP_PROJECT_CREATED");

            return appProject.Id;
        }

        public async Task DeleteAsync(long customerId, long id, CancellationToken cancellationToken)
        {
            var appProjects = await _dbContext.AppProjects.AsNoTracking().Where(x => x.Id == id && x.CustomerId == customerId).SingleOrDefaultAsync(cancellationToken);
            if (appProjects == null)
                throw new NotFoundException(nameof(AppProject));
            _dbContext.ChangeTracker.Clear();
            _dbContext.AppProjects.Remove(appProjects);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _bus.Publish("APP_PROJECT_DELETED");
        }

        public async Task PatchAsync(long id, long customerId, string title, Guid? appProjectViewId, AppProjectType appProjectType, int internalPort, RowState state, CancellationToken cancellationToken)
        {
            if (appProjectType == AppProjectType.UsePort && (!appProjectViewId.HasValue || appProjectViewId == Guid.Empty))
                throw new ArgumentNullException(nameof(appProjectViewId));

            var customer = await _dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == customerId, cancellationToken);
            if (customer == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), customerId) });

            var appProject = await _dbContext.AppProjects.SingleOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, cancellationToken);
            if (appProject == null)
                throw new NotFoundException(nameof(AppProject), new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>(nameof(AppProject.Id), id) });
             
            //Guid appProjectViewIdGuid = string.IsNullOrWhiteSpace(appProjectViewId) ? Guid.Empty : Guid.Parse(appProjectViewId);

            if (appProjectType == AppProjectType.UsePort && !_dbContext.AppProjects.Any(x => x.ViewId == appProjectViewId))
                throw new NotFoundException(nameof(AppProject), new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>(nameof(AppProject.AppProjectViewId), appProjectViewId.Value) });

            if (!string.IsNullOrWhiteSpace(title))
                appProject.Title = title;
            if (internalPort < 1 || internalPort > 65535)
                appProject.InternalPort = internalPort;

            appProject.AppProjectViewId = appProjectViewId; 
            appProject.AppProjectType = appProjectType;
            appProject.State = state;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _bus.Publish("APP_PROJECT_PATCHED");
        }
    }
}
