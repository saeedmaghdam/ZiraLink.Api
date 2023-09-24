using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.Tools;
using ZiraLink.Domain.Enums;
using ZiraLink.Api.Application.UnitTests.Tools;
using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Domain;

namespace ZiraLink.Api.Application.UnitTests.Services
{
    public class AppProjectServiceTests
    {
        private readonly TestTools _testTools;
        public AppProjectServiceTests()
        {
            _testTools = new TestTools();
            _testTools.Initialize(nameof(AppProjectServiceTests));
        }

        #region GetAppProject
        [Theory]
        [InlineData(1)]
        public async Task GetAppProject_WhenEverythingIsOk_ShouldHasData(long customerId)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService AppProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);

            var response = await AppProjectService.GetAsync(customerId, CancellationToken.None);

            Assert.True(response.Any());
            Assert.Equal("TestTitle1", response[0].Title);
            Assert.Equal(AppProjectType.SharePort, response[0].AppProjectType);
            Assert.Equal(2021, response[0].InternalPort);
        }

        [Theory]
        [InlineData(0)]
        public async Task GetAppProject_WhenCustomerIsNotExist_ShouldBeNull(long customerId)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService AppProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            var customer = _testTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.Null(customer);
            var response = await AppProjectService.GetAsync(customerId, CancellationToken.None);

            Assert.False(response.Any());
        }

        [Theory]
        [InlineData(4)]
        public async Task GetAppProject_WhenCustomerIsExistWithNoAppProjects_ShouldBeNull(long customerId)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService AppProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            var customer = _testTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.NotNull(customer);
            Assert.Equal("4", customer.ExternalId); 
            Assert.Equal("TestUser4", customer.Username);
            Assert.Equal("TestName4", customer.Name);
            Assert.Equal("User4", customer.Family);
            Assert.Equal("TestUser4@ZiraLink.com", customer.Email);
            var response = await AppProjectService.GetAsync(customerId, CancellationToken.None);

            Assert.False(response.Any());
        }
        #endregion

    }
}
