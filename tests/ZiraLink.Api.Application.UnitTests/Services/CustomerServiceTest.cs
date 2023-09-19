
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.UnitTests.Tools;
using ZiraLink.Domain;

namespace ZiraLink.Api.Application.UnitTests.Services
{ 
    public class CustomerServiceTest
    { 
        private readonly TestTools _testTools;
        public CustomerServiceTest()
        {
            _testTools = new TestTools();
            _testTools.Initialize(nameof(CustomerServiceTest));
        }

        #region GetCustomerByExternalId
        [Theory]
        [InlineData("4")]
        public async Task GetCustomerByExternalId_WhenEverythingIsOk_ShouldHasData(string externalId)
        {
            //Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(m => m["ZIRALINK_URL_IDS"]).Returns("https://ids.ziralink.local:5001");
   
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var response = await customerService.GetCustomerByExternalIdAsync(externalId, CancellationToken.None);
            Assert.True(response is Customer);
            Assert.Equal(externalId, response.ExternalId);
        }
        #endregion
    }
}
