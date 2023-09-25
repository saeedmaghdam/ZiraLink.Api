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
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);

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
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);
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
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);
            
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
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);

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
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);
            
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
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);
            
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
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);
            
            var customer = _testTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.Null(customer);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => appProjectService.GetByIdAsync(id, customerId, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }
        #endregion

        #region CreateAppProject
        [Theory]
        [InlineData(1, "TestTitle","00000000-0000-0000-0000-000000000000", AppProjectType.SharePort, 3030, ProjectState.Active)]
        public async Task CreateAppProject_WhenEverythingIsOk_ShouldBeSucceeded(long customerId, string title, object appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);

            mockBus.Setup(p => p.Publish(It.IsAny<string>()));

            var response = await appProjectService.CreateAsync(customerId, title, Guid.Parse(appProjectViewId.ToString()), appProjectType, internalPort, state, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, response);

            var createdRow = await _testTools.AppMemoryDbContext.AppProjects.Where(x => x.ViewId == response).FirstOrDefaultAsync();
            Assert.NotNull(createdRow);

            Assert.Equal(customerId, createdRow.CustomerId);
            Assert.Equal(title, createdRow.Title); 
            Assert.Equal(appProjectType, createdRow.AppProjectType);
            Assert.Equal(internalPort, createdRow.InternalPort);
            Assert.Equal(state, createdRow.State);
            mockBus.Verify(p => p.Publish("APP_PROJECT_CREATED"), Times.Once());
        }

        [Theory]
        [InlineData(1, "", "00000000-0000-0000-0000-000000000000", AppProjectType.SharePort, 3030, ProjectState.Active)]
        public async Task CreateAppProject_WhenTitleIsEmpty_ShouldBeFailed(long customerId, string title, object appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Assert.True(string.IsNullOrWhiteSpace(title));
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);
            
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => appProjectService.CreateAsync(customerId, title, Guid.Parse(appProjectViewId.ToString()), appProjectType, internalPort, state, CancellationToken.None));
            Assert.Equal("title", exception.ParamName);
        }

        [Theory]
        [InlineData(1, "TestTitle", "00000000-0000-0000-0000-000000000000", AppProjectType.UsePort, 3030, ProjectState.Active)]
        public async Task CreateAppProject_WhenAppProjectViewIdIsEmpty_ShouldBeFailed(long customerId, string title, object appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => appProjectService.CreateAsync(customerId, title, Guid.Parse(appProjectViewId.ToString()), appProjectType, internalPort, state, CancellationToken.None));
            Assert.Equal("appProjectViewId", exception.ParamName);
        }

        [Theory]
        [InlineData(1, "TestTitle", "00000000-0000-0000-0000-000000000000", AppProjectType.SharePort, 99999, ProjectState.Active)]
        public async Task CreateAppProject_WhenInternalPortIsNotValid_ShouldBeFailed(long customerId, string title, object appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Assert.True(internalPort < 1 || internalPort > 65535);

            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);

            var exception = await Assert.ThrowsAsync<ApplicationException>(() => appProjectService.CreateAsync(customerId, title, Guid.Parse(appProjectViewId.ToString()), appProjectType, internalPort, state, CancellationToken.None));
            Assert.Equal("Port range is not valid", exception.Message);
        }

        [Theory]
        [InlineData(0, "TestTitle", "00000000-0000-0000-0000-000000000000", AppProjectType.SharePort, 3030, ProjectState.Active)]
        public async Task CreateAppProject_WhenCustomerIdIsNotExist_ShouldBeFailed(long customerId, string title, object appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => appProjectService.CreateAsync(customerId, title, Guid.Parse(appProjectViewId.ToString()), appProjectType, internalPort, state, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }
        
        [Theory]
        [InlineData(1, "TestTitle", "af43138e-5f62-4dda-807d-1feed7913636", AppProjectType.UsePort, 3030, ProjectState.Active)]
        public async Task CreateAppProject_WhenAppProjectViewIdIsNotExist_ShouldBeFailed(long customerId, string title, object appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => appProjectService.CreateAsync(customerId, title, Guid.Parse(appProjectViewId.ToString()), appProjectType, internalPort, state, CancellationToken.None));
            Assert.Equal("AppProject", exception.Message);
        }

        #endregion

    }
}
