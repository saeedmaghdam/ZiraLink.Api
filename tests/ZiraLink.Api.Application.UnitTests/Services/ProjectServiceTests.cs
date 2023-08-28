using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.Tools;
using ZiraLink.Domain.Enums;
using ZiraLink.Api.Application.UnitTests.Tools;
namespace ZiraLink.Api.Application.UnitTests.Services
{
    public class ProjectServiceTests
    {
        private readonly ProjectService _projectService;

        private readonly Mock<ILogger<ProjectService>>? _mockILoggerProjectService;
        private readonly Mock<IBus> _mockIBus;
        private readonly Mock<IHttpClientFactory> _mockIHttpClientFactory;

        public ProjectServiceTests()
        {
            TestTools.Initialize();
            _mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            _mockIBus = new Mock<IBus>();
            _mockIHttpClientFactory = new Mock<IHttpClientFactory>();
            HttpTools httpTools = new HttpTools(_mockIHttpClientFactory.Object);
            _projectService = new ProjectService(_mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, _mockIBus.Object, httpTools);
        }

        [Theory]
        [InlineData(1, "TestTitle", DomainType.Default, "TestDomain", "https://localhost:3000", ProjectState.Active)]
        public async Task CreateProject_WhenEverythingIsOk_ShouldBeSucceeded(long customerId, string title, DomainType domainType, string domain, string internalUrl, ProjectState state)
        {
            _mockIBus.Setup(p => p.Publish(It.IsAny<string>()));
            var response = await _projectService?.CreateAsync(customerId, title, domainType, domain, internalUrl, state, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, response);

            var createdRow = await TestTools.AppMemoryDbContext.Projects.Where(x=> x.ViewId == response).FirstOrDefaultAsync();
            Assert.NotNull(createdRow);

            Assert.Equal(customerId, createdRow.CustomerId);
            Assert.Equal(title, createdRow.Title);
            Assert.Equal(domainType, createdRow.DomainType);
            Assert.Equal(domain, createdRow.Domain);
            Assert.Equal(internalUrl, createdRow.InternalUrl);
            Assert.Equal(state, createdRow.State);
            _mockIBus.Verify(p => p.Publish("CUSTOMER_CREATED"), Times.Once());
        }

    }
}
