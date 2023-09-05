﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.Tools;
using ZiraLink.Domain.Enums;
using ZiraLink.Api.Application.UnitTests.Tools;
using ZiraLink.Api.Application.Exceptions;

namespace ZiraLink.Api.Application.UnitTests.Services
{
    public class ProjectServiceTests
    {
        public ProjectServiceTests()
        {
            TestTools.Initialize();
        }

        #region CreateProject
        [Theory]
        [InlineData(1, "TestTitle", DomainType.Default, "TestDomain", "https://localhost:4000", RowState.Active)]
        public async Task CreateProject_WhenEverythingIsOk_ShouldBeSucceeded(long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            mockIBus.Setup(p => p.Publish(It.IsAny<string>()));
            mockIHttpTools.Setup(p => p.CheckDomainExists(It.IsAny<string>())).ReturnsAsync(true);

            var response = await projectService.CreateAsync(customerId, title, domainType, domain, internalUrl, state, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, response);

            var createdRow = await TestTools.AppMemoryDbContext.Projects.Where(x => x.ViewId == response).FirstOrDefaultAsync();
            Assert.NotNull(createdRow);

            Assert.Equal(customerId, createdRow.CustomerId);
            Assert.Equal(title, createdRow.Title);
            Assert.Equal(domainType, createdRow.DomainType);
            Assert.Equal(domain, createdRow.Domain);
            Assert.Equal(internalUrl, createdRow.InternalUrl);
            Assert.Equal(state, createdRow.State);
            mockIBus.Verify(p => p.Publish("PROJECT_CREATED"), Times.Once());
            mockIHttpTools.Verify(p => p.CheckDomainExists(It.IsAny<string>()), Times.Once());
        }

        [Theory]
        [InlineData(1, "", DomainType.Default, "TestDomain", "https://localhost:4000", RowState.Active)]
        public async Task CreateProject_WhenTitleIsEmpty_ShouldBeFailed(long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();

            Assert.True(string.IsNullOrWhiteSpace(title));
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => projectService.CreateAsync(customerId, title, domainType, domain, internalUrl, state, CancellationToken.None));
            Assert.Equal("title", exception.ParamName);
        }

        [Theory]
        [InlineData(1, "TestTitle", DomainType.Default, "", "https://localhost:4000", RowState.Active)]
        public async Task CreateProject_WhenDomainIsEmpty_ShouldBeFailed(long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();

            Assert.True(string.IsNullOrWhiteSpace(domain));
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => projectService.CreateAsync(customerId, title, domainType, domain, internalUrl, state, CancellationToken.None));
            Assert.Equal("domain", exception.ParamName);
        }

        [Theory]
        [InlineData(1, "TestTitle", DomainType.Default, "TestDomain", "", RowState.Active)]
        public async Task CreateProject_WhenInternalUrlIsEmpty_ShouldBeFailed(long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();

            Assert.True(string.IsNullOrWhiteSpace(internalUrl));
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => projectService.CreateAsync(customerId, title, domainType, domain, internalUrl, state, CancellationToken.None));
            Assert.Equal("internalUrl", exception.ParamName);
        }

        [Theory]
        [InlineData(0, "TestTitle", DomainType.Default, "TestDomain", "https://localhost:4000", RowState.Active)]
        public async Task CreateProject_WhenCustomerIdIsNotExist_ShouldBeFailed(long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            Assert.Equal(0, customerId);
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => projectService.CreateAsync(customerId, title, domainType, domain, internalUrl, state, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }

        [Theory]
        [InlineData(1, "TestTitle", DomainType.Default, "TestDomain2", "https://google.com", RowState.Active)]
        public async Task CreateProject_WhenInternalUrlIsAPublicUrl_ShouldBeFailed(long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            mockIHttpTools.Setup(p => p.CheckDomainExists(It.IsAny<string>())).ReturnsAsync(false);

            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ApplicationException>(() => projectService.CreateAsync(customerId, title, domainType, domain, internalUrl, state, CancellationToken.None));

            mockIHttpTools.Verify(p => p.CheckDomainExists(internalUrl), Times.Once());
            Assert.Equal("Public domain is not allowed", exception.Message);
        }
        #endregion

        #region GetProject
        [Theory]
        [InlineData(1)]
        public async Task GetProject_WhenEverythingIsOk_ShouldHasData(long customerId)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var response = await projectService.GetAsync(customerId, CancellationToken.None);
            Assert.True(response.Any());
        }

        [Theory]
        [InlineData(0)]
        public async Task GetProject_WhenCustomerIsNotExist_ShouldBeNull(long customerId)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);
            var customer = TestTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.Null(customer);
            var response = await projectService.GetAsync(customerId, CancellationToken.None);
            Assert.False(response.Any());
        }

        [Theory]
        [InlineData(4)]
        public async Task GetProject_WhenCustomerIsExistWithNoProjects_ShouldBeNull(long customerId)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);
            var customer = TestTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.NotNull(customer);
            var response = await projectService.GetAsync(customerId, CancellationToken.None);
            Assert.False(response.Any());
        }
        #endregion

        #region GetAllProjects
        [Fact]
        public async Task GetAllProjects_ShouldHasData()
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var response = await projectService.GetAllAsync(CancellationToken.None);
            Assert.True(response.Any());
        }
        #endregion

        #region GetByIdProject
        [Theory]
        [InlineData(1, 1)]
        public async Task GetByIdProject_WhenEverythingIsOk_ShouldHasData(long id, long customerId)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var response = await projectService.GetByIdAsync(id, customerId, CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
            Assert.Equal(customerId, response.CustomerId);
        }

        [Theory]
        [InlineData(0, 1)]
        public async Task GetByIdProject_WhenIdIsNotExist_ShouldBeNull(long id, long customerId)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var project = TestTools.AppMemoryDbContext.Projects.Find(id);
            Assert.Null(project);

            var customer = TestTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.NotNull(customer);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => projectService.GetByIdAsync(id, customerId, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }

        [Theory]
        [InlineData(1, 0)]
        public async Task GetByIdProject_WhenIdCustomerIdIsNotExist_ShouldBeNull(long id, long customerId)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var customer = TestTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.Null(customer);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => projectService.GetByIdAsync(id, customerId, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }
        #endregion

        #region DeleteProject
        [Theory]
        [InlineData(2, 2)]
        public async Task DeleteProject_WhenEverythingIsOk_ShouldBeSucceeded(long customerId, long id)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            mockIBus.Setup(p => p.Publish(It.IsAny<string>()));

            await projectService.DeleteAsync(customerId, id, CancellationToken.None);

            mockIBus.Verify(p => p.Publish("PROJECT_DELETED"), Times.Once());

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => projectService.GetByIdAsync(id, customerId, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }

        [Theory]
        [InlineData(2, 0)]
        public async Task DeleteProject_WhenIdIsNotExist_ShouldBeFailed(long customerId, long id)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var project = TestTools.AppMemoryDbContext.Projects.Find(id);
            Assert.Null(project);

            var customer = TestTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.NotNull(customer);

            await Assert.ThrowsAsync<NotFoundException>(() => projectService.DeleteAsync(customerId, id, CancellationToken.None));

        }
        [Theory]
        [InlineData(0, 0)]
        public async Task DeleteProject_WhenCustomerIdIsNotExist_ShouldBeFailed(long customerId, long id)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var customer = TestTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.Null(customer);

            await Assert.ThrowsAsync<NotFoundException>(() => projectService.DeleteAsync(customerId, id, CancellationToken.None));

        }
        #endregion

        #region PatchProject
        [Theory]
        [InlineData(3, 3, "TestTitleEdited", DomainType.Default, "TestDomainEdited", "https://localhost:4001", RowState.Active)]
        public async Task PatchProject_WhenEverythingIsOk_ShouldBeSucceeded(long id, long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            mockIBus.Setup(p => p.Publish(It.IsAny<string>()));
            mockIHttpTools.Setup(p => p.CheckDomainExists(It.IsAny<string>())).ReturnsAsync(true);

            await projectService.PatchAsync(id, customerId, title, domainType, domain, internalUrl, state, CancellationToken.None);

            Assert.True(true);
            mockIBus.Verify(p => p.Publish("PROJECT_PATCHED"), Times.Once());
            mockIHttpTools.Verify(p => p.CheckDomainExists(It.IsAny<string>()), Times.Once());
        }

        [Theory]
        [InlineData(3, 0, "TestTitleEdited", DomainType.Default, "TestDomainEdited", "https://localhost:4001", RowState.Active)]
        public async Task PatchProject_WhenCustomerIdIsNotExist_ShouldBeFailed(long id, long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var customer = TestTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.Null(customer);
            
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => projectService.PatchAsync(id, customerId, title, domainType, domain, internalUrl, state, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }

        [Theory]
        [InlineData(0, 3, "TestTitleEdited", DomainType.Default, "TestDomainEdited", "https://localhost:4001", RowState.Active)]
        public async Task PatchProject_WhenIdIsNotExist_ShouldBeFailed(long id, long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            var customer = TestTools.AppMemoryDbContext.Customers.Find(customerId);
            Assert.NotNull(customer);
            
            var project = TestTools.AppMemoryDbContext.Projects.Find(id);
            Assert.Null(project);
            
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => projectService.PatchAsync(id, customerId, title, domainType, domain, internalUrl, state, CancellationToken.None));
            Assert.Equal("Project", exception.Message);
        }

        [Theory]
        [InlineData(3, 3, "TestTitleEdited", DomainType.Default, "TestDomain1", "https://localhost:3001", RowState.Active)]
        public async Task PatchProject_WhenDomainAlreadyExists_ShouldBeFailed(long id, long customerId, string title, DomainType domainType, string domain, string internalUrl, RowState state)
        {
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);
 
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => projectService.PatchAsync(id, customerId, title, domainType, domain, internalUrl, state, CancellationToken.None));
            Assert.Equal("Domain already exists", exception.Message);
        }
        #endregion

    }
}
