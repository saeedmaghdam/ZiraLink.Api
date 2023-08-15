using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.Application
{
    public class ProjectService : IProjectService
    {
        private readonly ILogger<ProjectService> _logger;
        private readonly AppDbContext _dbContext;

        public ProjectService(ILogger<ProjectService> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<List<Project>> GetAsync(long customerId, CancellationToken cancellationToken)
        {
            return await _dbContext.Projects.Include(x => x.Customer).AsNoTracking().Where(x => x.Customer.Id == customerId).ToListAsync(cancellationToken);
        }

        public async Task<Guid> CreateAsync(long customerId, string title, DomainType domainType, string domain, string internalUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException(nameof(title));
            if (string.IsNullOrEmpty(domain))
                throw new ArgumentNullException(nameof(domain));
            if (string.IsNullOrEmpty(internalUrl))
                throw new ArgumentNullException(nameof(internalUrl));

            var customer = await _dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == customerId, cancellationToken);
            if (customer == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), customerId) });

            var project = new Project
            {
                ViewId = Guid.NewGuid(),
                CustomerId = customer.Id,
                Title = title,
                DomainType = domainType,
                Domain = domain,
                InternalUrl = internalUrl,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            await _dbContext.Projects.AddAsync(project, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return project.ViewId;
        }

        public async Task DeleteAsync(long customerId, long id, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects.AsNoTracking().Where(x => x.Id == id && x.CustomerId == customerId).SingleOrDefaultAsync(cancellationToken);
            if (project == null)
                throw new NotFoundException(nameof(Project));

            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
