using Microsoft.Extensions.Logging;
using Moq;
using ZiraLink.Api.Application;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.Tools;
using ZiraLink.Domain.Enums;
using ZiraLink.UnitTests.Tools;

namespace ZiraLink.UnitTests.Services
{
    public class ProjectService_UnitTest
    {
        private readonly ProjectService? _projectService;

        private readonly Mock<ILogger<ProjectService>>? _mockILoggerProjectService;
        private readonly Mock<IBus>? _mockIBus;
        private readonly Mock<IHttpClientFactory>? _mockIHttpClientFactory;
         
        public ProjectService_UnitTest()
        {
            TestTools.Initialize();
            _mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            _mockIBus = new Mock<IBus>();
            _mockIHttpClientFactory = new Mock<IHttpClientFactory>();
            HttpTools httpTools = new HttpTools(_mockIHttpClientFactory.Object);
            _projectService = new ProjectService(_mockILoggerProjectService.Object, TestTools._dbContext, _mockIBus.Object, httpTools);
        }
       
        [Theory]
        [InlineData(1, "TestTitle", DomainType.Default, "TestDomain", "https://localhost:3000", ProjectState.Active)]
        public async Task CreateProject_WhenEverythingIsOk_ShouldBeSucceeded(long customerId, string title, DomainType domainType, string domain, string internalUrl, ProjectState state)
        {
            var response = await _projectService?.CreateAsync(customerId, title, domainType, domain, internalUrl, state, CancellationToken.None);
             
            Assert.NotEqual("", response.ToString());
        }

    }
}
