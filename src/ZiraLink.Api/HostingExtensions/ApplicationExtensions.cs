using Microsoft.EntityFrameworkCore;
using ZiraLink.Api.Application;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.HostingExtensions
{
    public static class ApplicationExtensions
    {
        public static async Task InitializeTestEnvironmentAsync(this WebApplication app)
        {
            using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var customer = await appDbContext.Customers.FirstOrDefaultAsync(x => x.Username == "logon", cancellationTokenSource.Token);
            if (customer == null)
            {
                var customerExternalId = "c2bacf97-47ab-452d-ba04-937a001f72ac";

                customer = new Customer
                {
                    Username = "test",
                    Name = "Test",
                    Family = "User",
                    Email = "ziralink@aghdam.nl",
                    ViewId = Guid.NewGuid(),
                    ExternalId = Guid.Parse(customerExternalId).ToString()
                };
                appDbContext.Customers.Add(customer);
            }

            if (!await appDbContext.Projects.AnyAsync(x => x.DomainType == DomainType.Custom && x.Domain == "localhost", cancellationTokenSource.Token))
            {
                appDbContext.Projects.Add(new Domain.Project
                {
                    ViewId = Guid.NewGuid(),
                    Title = "Weather Forecasts",
                    State = Domain.Enums.ProjectState.Active,
                    InternalUrl = "https://host.docker.internal:10051",
                    Customer = customer,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    DomainType = DomainType.Custom,
                    Domain = "localhost"
                });
            }

            if (appDbContext.ChangeTracker.HasChanges())
                await appDbContext.SaveChangesAsync(cancellationTokenSource.Token);
        }
    }
}
