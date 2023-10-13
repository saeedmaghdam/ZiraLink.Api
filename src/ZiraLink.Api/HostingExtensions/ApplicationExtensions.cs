using ZiraLink.Api.Application;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.HostingExtensions
{
    public static class ApplicationExtensions
    {
        public static async Task InitializeTestEnvironmentAsync(this WebApplication app, IConfiguration configuration)
        {
            if (configuration["ASPNETCORE_ENVIRONMENT"] != "Test")
                return;

            using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var customerExternalId = "c2bacf97-47ab-452d-ba04-937a001f72ac";

            var customer = new Customer
            {
                Username = "test",
                Name = "Test",
                Family = "User",
                Email = "ziralink@aghdam.nl",
                ViewId = Guid.NewGuid(),
                ExternalId = Guid.Parse(customerExternalId).ToString()
            };
            await appDbContext.Customers.AddAsync(customer);

            await appDbContext.Projects.AddAsync(new Domain.Project
            {
                ViewId = Guid.NewGuid(),
                Title = "Weather Forecasts",
                State = Domain.Enums.ProjectState.Active,
                InternalUrl = "https://swa.localdev.me:10051",
                Customer = customer,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                DomainType = DomainType.Custom,
                Domain = "localhost"
            });

            var tcpProjectViewId = Guid.NewGuid();
            await appDbContext.AppProjects.AddAsync(new AppProject
            {
                ViewId = tcpProjectViewId,
                AppProjectType = AppProjectType.SharePort,
                Customer = customer,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                InternalPort = 5511,
                PortType = PortType.TCP,
                State = ProjectState.Active,
                Title = "Share 5511 TCP"
            });

            await appDbContext.AppProjects.AddAsync(new AppProject
            {
                ViewId = Guid.NewGuid(),
                AppProjectViewId = tcpProjectViewId,
                AppProjectType = AppProjectType.UsePort,
                Customer = customer,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                InternalPort = 5512,
                PortType = PortType.TCP,
                State = ProjectState.Active,
                Title = "Use 5511 TCP"
            });

            var udpProjectViewId = Guid.NewGuid();
            await appDbContext.AppProjects.AddAsync(new AppProject
            {
                ViewId = udpProjectViewId,
                AppProjectType = AppProjectType.SharePort,
                Customer = customer,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                InternalPort = 5521,
                PortType = PortType.UDP,
                State = ProjectState.Active,
                Title = "Share 5521 UDP"
            });

            await appDbContext.AppProjects.AddAsync(new AppProject
            {
                ViewId = Guid.NewGuid(),
                AppProjectViewId = udpProjectViewId,
                AppProjectType = AppProjectType.UsePort,
                Customer = customer,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                InternalPort = 5522,
                PortType = PortType.UDP,
                State = ProjectState.Active,
                Title = "Use 5521 UDP"
            });

            await appDbContext.SaveChangesAsync(cancellationTokenSource.Token);
        }
    }
}
