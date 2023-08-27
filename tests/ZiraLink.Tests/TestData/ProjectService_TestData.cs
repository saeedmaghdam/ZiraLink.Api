using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZiraLink.Api.Models.Project.InputModels;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Tests.TestData
{
    public class ProjectService_TestData
    {
        public static IEnumerable<object[]> SetDataFor_CreateProject_WithEverythingIsOk()
        {
            yield return new object[] { new CreateProjectInputModel() {
                    Title="Test",
                    DomainType = Domain.Enums.DomainType.Default,
                    Domain = "Test",
                    InternalUrl = "http://test.com",
                    State  = ProjectState.Active,
        }
    };
        }

    }
}
