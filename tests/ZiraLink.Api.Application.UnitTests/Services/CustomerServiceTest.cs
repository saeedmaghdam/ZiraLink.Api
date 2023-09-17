
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.UnitTests.Tools;
using ZiraLink.Domain;

namespace ZiraLink.Api.Application.UnitTests.Services
{
    public class CustomerServiceTest : IClassFixture<TestInitalizeFixture>
    { 
        private readonly TestInitalizeFixture _testInitalizeFixture;
        public CustomerServiceTest()
        { 
            //_testInitalizeFixture = testInitalizeFixture;
        }

        #region GetCustomerByExternalId
        [Theory]
        [InlineData("4")]
        public async Task GetCustomerByExternalId_WhenEverythingIsOk_ShouldHasData(string externalId)
        {
            //Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(m => m["ZIRALINK_URL_IDS"]).Returns("https://ids.ziralink.local:5001");
   
            CustomerService customerService = new CustomerService(_testInitalizeFixture.AppMemoryDbContext, mockConfiguration.Object);

            var response = await customerService.GetCustomerByExternalIdAsync(externalId, CancellationToken.None);
            Assert.True(response is Customer);
            Assert.Equal(externalId, response.ExternalId);
        }
        #endregion
    }
}
