using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Api.Application.Tools;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ILogger<ProjectService> _logger;
        private readonly AppDbContext _dbContext;
        private readonly IBus _bus;
        private readonly IHttpTools _httpTools;

        public ProjectService(ILogger<ProjectService> logger, AppDbContext dbContext, IBus bus, IHttpTools httpTools)
        {
            _logger = logger;
            _dbContext = dbContext;
            _bus = bus;
            _httpTools = httpTools;
        }

        public async Task<List<Project>> GetAsync(long customerId, CancellationToken cancellationToken)
        {
            return await _dbContext.Projects.Include(x => x.Customer).AsNoTracking().Where(x => x.Customer.Id == customerId).ToListAsync(cancellationToken);
        }

        public async Task<List<Project>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Projects.Include(x => x.Customer).AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Project> GetByIdAsync(long id, long customerId, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects.Include(x => x.Customer).AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.Customer.Id == customerId, cancellationToken);
            if (project == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), customerId) });

            return project;
        }

        public async Task<Guid> CreateAsync(long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));
            if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentNullException(nameof(domain));
            if (string.IsNullOrWhiteSpace(internalUrl))
                throw new ArgumentNullException(nameof(internalUrl));


            var customer = await _dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == customerId, cancellationToken);
            if (customer == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), customerId) });

            var isDomainExists = await _dbContext.Projects.AnyAsync(x => x.DomainType == domainType && x.Domain == domain, cancellationToken);
            if (isDomainExists)
                throw new ApplicationException("Domain already exists");

            if (!await _httpTools.CheckDomainExists(internalUrl))
                throw new ApplicationException("Public domain is not allowed");

            var project = new Project
            {
                ViewId = Guid.NewGuid(),
                CustomerId = customer.Id,
                Title = title,
                DomainType = domainType,
                Domain = domain,
                InternalUrl = internalUrl,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                State = state
            };

            await _dbContext.Projects.AddAsync(project, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _bus.Publish("PROJECT_CREATED");

            return project.ViewId;
        }

        public async Task DeleteAsync(long customerId, long id, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects.AsNoTracking().Where(x => x.Id == id && x.CustomerId == customerId).SingleOrDefaultAsync(cancellationToken);
            if (project == null)
                throw new NotFoundException(nameof(Project));
            _dbContext.ChangeTracker.Clear();
            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _bus.Publish("PROJECT_DELETED");
        }

        public async Task PatchAsync(long id, long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state, CancellationToken cancellationToken)
        {
            var customer = await _dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == customerId, cancellationToken);
            if (customer == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), customerId) });

            var project = await _dbContext.Projects.SingleOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, cancellationToken);
            if (project == null)
                throw new NotFoundException(nameof(Project), new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>(nameof(Project.Id), id) });

            if (project.DomainType != domainType || project.Domain != domain)
            {
                var isDomainExists = await _dbContext.Projects.AnyAsync(x => x.DomainType == domainType && x.Domain == domain, cancellationToken);
                if (isDomainExists)
                    throw new ApplicationException("Domain already exists");
            }

            if (!await _httpTools.CheckDomainExists(internalUrl))
                throw new ApplicationException("Public domain is not allowed");

            if (!string.IsNullOrWhiteSpace(title))
                project.Title = title;
            if (!string.IsNullOrWhiteSpace(domain))
                project.Domain = domain;
            if (!string.IsNullOrWhiteSpace(internalUrl))
                project.InternalUrl = internalUrl;
            project.DomainType = domainType;
            project.State = state;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _bus.Publish("PROJECT_PATCHED");
        }
    }
}
