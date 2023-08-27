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
            var appDbContext = app.Services.GetRequiredService<AppDbContext>();
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

            if (!await appDbContext.Projects.AnyAsync(x => x.DomainType == DomainType.Default && x.Domain == "weather", cancellationTokenSource.Token))
            {
                appDbContext.Projects.Add(new Domain.Project
                {
                    ViewId = Guid.NewGuid(),
                    Title = "Weather Forecasts",
                    State = Domain.Enums.ProjectState.Active,
                    InternalUrl = "https://localhost:9443",
                    Customer = customer,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    DomainType = DomainType.Default,
                    Domain = "weather"
                });
            }

            if (!await appDbContext.Projects.AnyAsync(x => x.DomainType == DomainType.Default && x.Domain == "api", cancellationTokenSource.Token))
            {
                appDbContext.Projects.Add(new Domain.Project
                {
                    ViewId = Guid.NewGuid(),
                    Title = "Ziralink Api Swagger",
                    State = Domain.Enums.ProjectState.Active,
                    InternalUrl = "https://localhost:6101",
                    Customer = customer,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    DomainType = DomainType.Default,
                    Domain = "api"
                });
            }

            if (!await appDbContext.Projects.AnyAsync(x => x.DomainType == DomainType.Default && x.Domain == "ot", cancellationTokenSource.Token))
            {
                appDbContext.Projects.Add(new Domain.Project
                {
                    ViewId = Guid.NewGuid(),
                    Title = "Online Trading",
                    State = Domain.Enums.ProjectState.Active,
                    InternalUrl = "https://localhost:3001",
                    Customer = customer,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    DomainType = DomainType.Default,
                    Domain = "ot"
                });
            }

            await appDbContext.SaveChangesAsync(cancellationTokenSource.Token);
        }
    }
}
