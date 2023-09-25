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
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);

            var response = await appProjectService.GetAsync(customerId, CancellationToken.None);

            Assert.True(response.Any()); 
            Assert.Equal(customerId, response[0].CustomerId); 
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
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            var customer = _testTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.Null(customer);
            var response = await appProjectService.GetAsync(customerId, CancellationToken.None);

            Assert.False(response.Any());
        }

        [Theory]
        [InlineData(4)]
        public async Task GetAppProject_WhenCustomerIsExistWithNoAppProjects_ShouldBeNull(long customerId)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            var customer = _testTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.NotNull(customer);
            Assert.Equal("4", customer.ExternalId); 
            Assert.Equal("TestUser4", customer.Username);
            Assert.Equal("TestName4", customer.Name);
            Assert.Equal("User4", customer.Family);
            Assert.Equal("TestUser4@ZiraLink.com", customer.Email);
            var response = await appProjectService.GetAsync(customerId, CancellationToken.None);

            Assert.False(response.Any());
        }
        #endregion

        #region GetAllAppProjects
        [Fact]
        public async Task GetAllAppProjects_ShouldHasData()
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);

            var response = await appProjectService.GetAllAsync(CancellationToken.None);
 
            Assert.True(response.Any());
        }
        #endregion

        #region GetByIdAppProject
        [Theory]
        [InlineData(1, 1)]
        public async Task GetByIdAppProject_WhenEverythingIsOk_ShouldHasData(long id, long customerId)
        {
           Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            
            var response = await appProjectService.GetByIdAsync(id, customerId, CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
            Assert.Equal(customerId, response.CustomerId); 
            Assert.Equal("TestTitle1", response.Title);
            Assert.Equal(AppProjectType.SharePort, response.AppProjectType);
            Assert.Equal(2021, response.InternalPort);
        }

        [Theory]
        [InlineData(0, 1)]
        public async Task GetByIdAppProject_WhenIdIsNotExist_ShouldBeNull(long id, long customerId)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            
            var appProjects = _testTools.AppMemoryDbContext.AppProjects.Find(id);
            Assert.Null(appProjects);

            var customer = _testTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.NotNull(customer);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => appProjectService.GetByIdAsync(id, customerId, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }

        [Theory]
        [InlineData(1, 0)]
        public async Task GetByIdAppProject_WhenIdCustomerIdIsNotExist_ShouldBeNull(long id, long customerId)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            
            var customer = _testTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.Null(customer);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => appProjectService.GetByIdAsync(id, customerId, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }
        #endregion

    }
}
