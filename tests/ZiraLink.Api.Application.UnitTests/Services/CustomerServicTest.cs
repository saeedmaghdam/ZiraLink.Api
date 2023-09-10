
using Microsoft.Extensions.Configuration;
using Moq;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.UnitTests.Tools;
using ZiraLink.Domain;

namespace ZiraLink.Api.Application.UnitTests.Services
{
    public class CustomerServicTest
    {
        public CustomerServicTest()
        {
            TestTools.Initialize();
        }

        #region GetCustomerByExternalId
        [Theory]
        [InlineData("1")]
        public async Task GetCustomerByExternalId_WhenEverythingIsOk_ShouldHasData(string externalId)
        {
            //Arrange
            var myConfiguration = new Dictionary<string, string> { { "ZIRALINK_URL_IDS", "https://ids.ziralink.local:5001" } };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(myConfiguration).Build();

            CustomerService customerService = new CustomerService(TestTools.AppMemoryDbContext, configuration);

            var response = await customerService.GetCustomerByExternalIdAsync(externalId, CancellationToken.None);
            Assert.True(response is Customer);
            Assert.Equal(externalId, response.ExternalId);
        }
        #endregion
    }
}
