using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Tests.TestData
{
    public class ProjectService_TestData
    {
        public static IEnumerable<object[]> SetDataFor_CreateProject_WithEverythingIsOk()
        {
            yield return new object[] { new Project() {
                    Id = 1,
                    ViewId = new Guid(),
                    CustomerId = 1,
                    Title="Test",
                    DomainType = Domain.Enums.DomainType.Default,
                    Domain = "Test",
                    InternalUrl = "http://test.com",
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    State  = ProjectState.Active,
        }
    };
        }

    }
}
