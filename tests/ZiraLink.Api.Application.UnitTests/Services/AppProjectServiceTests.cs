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

        #region CreateAppProject
        [Theory]
        [InlineData(1, "TestTitle", DomainType.Default, "TestDomain", "https://localhost:4000", ProjectState.Active)]
        public async Task CreateAppProject_WhenEverythingIsOk_ShouldBeSucceeded(long customerId, string title, Guid? appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            
            mockBus.Setup(p => p.Publish(It.IsAny<string>()));
            mockHttpTools.Setup(p => p.CheckDomainExists(It.IsAny<string>())).ReturnsAsync(true);

            var response = await appProjectService.CreateAsync(customerId, title, appProjectViewId, appProjectType, internalPort, state, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, response);

            var createdRow = await _testTools.AppMemoryDbContext.AppProjects.Where(x => x.ViewId == response).FirstOrDefaultAsync();
            Assert.NotNull(createdRow);

            Assert.Equal(customerId, createdRow.CustomerId);
            Assert.Equal(title, createdRow.Title); 
            Assert.Equal(appProjectType, createdRow.AppProjectType);
            Assert.Equal(internalPort, createdRow.InternalPort);
            Assert.Equal(state, createdRow.State);
            mockBus.Verify(p => p.Publish("PROJECT_CREATED"), Times.Once());
            mockHttpTools.Verify(p => p.CheckDomainExists(It.IsAny<string>()), Times.Once());
        }

        [Theory]
        [InlineData(1, "", DomainType.Default, "TestDomain", "https://localhost:4000", ProjectState.Active)]
        public async Task CreateAppProject_WhenTitleIsEmpty_ShouldBeFailed(long customerId, string title, Guid? appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            Assert.True(string.IsNullOrWhiteSpace(title));
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => appProjectService.CreateAsync(customerId, title, appProjectViewId, appProjectType, internalPort, state, CancellationToken.None));
            Assert.Equal("title", exception.ParamName);
        }

        [Theory]
        [InlineData(1, "TestTitle", DomainType.Default, "", "https://localhost:4000", ProjectState.Active)]
        public async Task CreateAppProject_WhenDomainIsEmpty_ShouldBeFailed(long customerId, string title, Guid? appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            Assert.True(string.IsNullOrWhiteSpace(domain)); 

            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => appProjectService.CreateAsync(customerId, title, appProjectViewId, appProjectType, internalPort, state, CancellationToken.None));
            Assert.Equal("domain", exception.ParamName);
        }

        [Theory]
        [InlineData(1, "TestTitle", DomainType.Default, "TestDomain", "", ProjectState.Active)]
        public async Task CreateAppProject_WhenInternalUrlIsEmpty_ShouldBeFailed(long customerId, string title, Guid? appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            Assert.True(string.IsNullOrWhiteSpace(internalUrl)); 

            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => appProjectService.CreateAsync(customerId, title, appProjectViewId, appProjectType, internalPort, state, CancellationToken.None));
            Assert.Equal("internalUrl", exception.ParamName);
        }

        [Theory]
        [InlineData(0, "TestTitle", DomainType.Default, "TestDomain", "https://localhost:4000", ProjectState.Active)]
        public async Task CreateAppProject_WhenCustomerIdIsNotExist_ShouldBeFailed(long customerId, string title, Guid? appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => appProjectService.CreateAsync(customerId, title, appProjectViewId, appProjectType, internalPort, state, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }

        [Theory]
        [InlineData(1, "TestTitle", DomainType.Default, "TestDomain1", "https://localhost:4000", ProjectState.Active)]
        public async Task CreateAppProject_WhenDomainAlreadyExists_ShouldBeFailed(long customerId, string title, Guid? appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)

        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => appProjectService.CreateAsync(customerId, title, appProjectViewId, appProjectType, internalPort, state, CancellationToken.None));
            Assert.Equal("Domain already exists", exception.Message);
        }

        [Theory]
        [InlineData(1, "TestTitleN", DomainType.Default, "TestDomainN", "https://google.com", ProjectState.Active)]
        public async Task CreateAppProject_WhenInternalUrlIsAPublicUrl_ShouldBeFailed(long customerId, string title, Guid? appProjectViewId, AppProjectType appProjectType, int internalPort, ProjectState state)
        {
            Mock<ILogger<AppProjectService>> mockLoggerAppProjectService = new Mock<ILogger<AppProjectService>>();
            Mock<IBus> mockBus = new Mock<IBus>();
            Mock<IHttpTools> mockHttpTools = new Mock<IHttpTools>();
            AppProjectService appProjectService = new AppProjectService(mockLoggerAppProjectService.Object, _testTools.AppMemoryDbContext, mockBus.Object, mockHttpTools.Object);
            
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => appProjectService.CreateAsync(customerId, title, appProjectViewId, appProjectType, internalPort, state, CancellationToken.None));

            mockHttpTools.Verify(p => p.CheckDomainExists(internalUrl), Times.Once());
            Assert.Equal("Public domain is not allowed", exception.Message);
        }
        #endregion

    }
}
