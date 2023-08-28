using ZiraLink.Domain.Enums;

namespace ZiraLink.UnitTests.TestData
{
    public class ProjectService_TestData
    {
        public static IEnumerable<object[]> SetDataFor_CreateProject_WithEverythingIsOk()
        {
            yield return new object[] { 1, "TestTitle", DomainType.Default, "TestDomain", "testdomain.com", ProjectState.Active };
        }

    }
}
