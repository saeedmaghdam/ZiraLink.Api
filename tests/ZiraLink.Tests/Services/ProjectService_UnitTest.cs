using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using ZiraLink.Api.Application;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.Tools;
using ZiraLink.Api.Models.Project.InputModels;
using ZiraLink.Domain;
using ZiraLink.Tests.TestData;
using ZiraLink.Tests.Tools;

namespace ZiraLink.Tests.Services
{
    public class ProjectService_UnitTest
    {
        private readonly ProjectService _projectService;

        private readonly Mock<ILogger<ProjectService>>? _mockILoggerProjectService;
        private readonly Mock<IBus>? _mockIBus;
        private readonly Mock<IHttpTools>? _mockIHttpTools;
         
        public ProjectService_UnitTest()
        {
            TestTools.Initialize();
            _mockILoggerProjectService = new Mock<ILogger<ProjectService>>();
            _mockIBus = new Mock<IBus>(); 
            _mockIHttpTools = new Mock<IHttpTools>();
            _projectService = new ProjectService(_mockILoggerProjectService.Object, TestTools.dbContext, _mockIBus.Object, _mockIHttpTools.Object);
        }
       
        [Theory]
        [MemberData(nameof(ProjectService_TestData.SetDataFor_CreateProject_WithEverythingIsOk), MemberType = typeof(ProjectService_TestData))]
        public async Task CreateProject_WhenEverythingIsOk_ShouldBeSucceeded(CreateProjectInputModel requestData)
        {
            var response = await _projectService.CreateAsync(TestTools.customerId, requestData.Title, requestData.DomainType, requestData.Domain, requestData.InternalUrl, requestData.State, CancellationToken.None);
             
            Assert.NotEqual("", response.ToString());
        }

    }
}
