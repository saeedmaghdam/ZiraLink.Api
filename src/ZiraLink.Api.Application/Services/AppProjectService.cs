using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<Guid> CreateAsync(long customerId, string title, AppProjectType appProjectType, string appUniqueName, int internalPort, RowState state, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException(nameof(title));
            if (string.IsNullOrEmpty(appUniqueName))
                throw new ArgumentNullException(nameof(appUniqueName));
            if (internalPort <= 0 && internalPort > 99999)
                throw new ApplicationException("Port range is not valid");


            var customer = await _dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == customerId, cancellationToken);
            if (customer == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), customerId) });

            var isDomainExists = await _dbContext.AppProjects.AnyAsync(x => x.AppProjectType == appProjectType && x.AppUniqueName == appUniqueName, cancellationToken);
            if (isDomainExists)
                throw new ApplicationException("App name is already exists");

            var appProject = new AppProject
            {
                ViewId = Guid.NewGuid(),
                CustomerId = customer.Id,
                Title = title,
                AppProjectType = appProjectType,
                AppUniqueName = appUniqueName,
                InternalPort = internalPort,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                State = state
            };

            await _dbContext.AppProjects.AddAsync(appProject, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _bus.Publish("APP_PROJECT_CREATED");

            return appProject.ViewId;
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

        public async Task PatchAsync(long id, long customerId, string title, AppProjectType appProjectType, string appUniqueName, int internalPort, RowState state, CancellationToken cancellationToken)
        {
            var customer = await _dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == customerId, cancellationToken);
            if (customer == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), customerId) });

            var appProject = await _dbContext.AppProjects.SingleOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, cancellationToken);
            if (appProject == null)
                throw new NotFoundException(nameof(Project), new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>(nameof(Project.Id), id) });

            if (appProject.AppProjectType != appProjectType || appProject.AppUniqueName.ToLower() != appUniqueName.ToLower())
            {
                var isDomainExists = await _dbContext.AppProjects.AnyAsync(x => x.AppProjectType == appProjectType && x.AppUniqueName == appUniqueName, cancellationToken);
                if (isDomainExists)
                    throw new ApplicationException("App name is already exists");
            }
             
            if (!string.IsNullOrEmpty(title))
                appProject.Title = title;
            if (!string.IsNullOrEmpty(appUniqueName))
                appProject.AppUniqueName = appUniqueName;
            if (internalPort != 0)
                appProject.InternalPort = internalPort;
            appProject.AppProjectType = appProjectType;
            appProject.State = state;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _bus.Publish("APP_PROJECT_PATCHED");
        }
    }
}
