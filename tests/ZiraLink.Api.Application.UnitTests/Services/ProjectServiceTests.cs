﻿using Microsoft.EntityFrameworkCore;
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


        public ProjectServiceTests()
        {
            TestTools.Initialize();
        }

        [Theory]
        [InlineData(1, "TestTitle", DomainType.Default, "TestDomain", "https://localhost:3000", ProjectState.Active)]
        public async Task CreateProject_WhenEverythingIsOk_ShouldBeSucceeded(long customerId, string title, DomainType domainType, string domain, string internalUrl, ProjectState state)
        {  
            Mock<ILogger<ProjectService>> mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            Mock<IBus> mockIBus = new Mock<IBus>();
            Mock<IHttpTools> mockIHttpTools = new Mock<IHttpTools>();
            ProjectService projectService = new ProjectService(mockILoggerProjectService.Object, TestTools.AppMemoryDbContext, mockIBus.Object, mockIHttpTools.Object);

            mockIBus.Setup(p => p.Publish(It.IsAny<string>()));
            mockIHttpTools.Setup(p => p.CheckDomainExists(It.IsAny<string>())).Returns(Task.FromResult(true));

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
            mockIBus.Verify(p => p.Publish("CUSTOMER_CREATED"), Times.Once());
        }

    }
}