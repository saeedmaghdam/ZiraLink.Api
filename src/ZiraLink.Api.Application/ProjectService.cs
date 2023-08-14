using System.Linq.Expressions;
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

        public async Task<List<Project>> GetAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Projects.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Guid> CreateAsync(long id, string title, DomainType domainType, string domain, string internalUrl, CancellationToken cancellationToken)
        {
            var customer = await _dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (customer == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), id)});

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
    }
}
