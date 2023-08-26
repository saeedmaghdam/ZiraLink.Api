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
        [Fact]
        public async Task Test1()
        {
            var a = await _projectService.GetAsync(1, CancellationToken.None);
            Assert.True(a != null);
        }
    }
}
