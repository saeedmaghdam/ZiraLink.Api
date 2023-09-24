
using Castle.Core.Resource;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Moq;
using Xunit;

using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.Tools;
using ZiraLink.Api.Application.UnitTests.Tools;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

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
            mockConfiguration.Setup(m => m[It.IsAny<string>()]).Returns("https://ids.ziralink.local:5001");

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var response = await customerService.GetCustomerByExternalIdAsync(externalId, CancellationToken.None);
            Assert.True(response is Customer);
            Assert.Equal(externalId, response.ExternalId);
        }

        [Theory]
        [InlineData("100")]
        public async Task GetCustomerByExternalId_WhenExternalIdIsNotExist_ShouldHasData(string externalId)
        {
            //Arrange
            var mockConfiguration = new Mock<IConfiguration>(); 
            mockConfiguration.Setup(m => m[It.IsAny<string>()]).Returns("https://ids.ziralink.local:5001");

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => customerService.GetCustomerByExternalIdAsync(externalId, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }
        #endregion

        #region CreateLocallyAsync
        [Theory]
        [InlineData("100", "TestUser", "TestEmail@email.com", "Test", "Testi")]
        public async Task CreateLocallyAsync_WhenEverythingIsOk_ShouldBeSucceeded(string externalId, string username, string email, string name, string family)
        {
            var mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(m => m[It.IsAny<string>()]).Returns("https://ids.ziralink.local:5001");
            
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);
              
            var response = await customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, response);

            var createdRow = await _testTools.AppMemoryDbContext.Customers.Where(x => x.ViewId == response).FirstOrDefaultAsync();
            Assert.NotNull(createdRow);

            Assert.Equal(username, createdRow.Username);
            Assert.Equal(email, createdRow.Email);
            Assert.Equal(name, createdRow.Name);
            Assert.Equal(family, createdRow.Family);  
        }

        [Theory]
        [InlineData("100", "", "TestEmail@email.com", "Test", "Testi")]
        public async Task CreateLocallyAsync_WhenUsernameIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(m => m[It.IsAny<string>()]).Returns("https://ids.ziralink.local:5001");

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("username", exception.ParamName);
        }
        
        [Theory]
        [InlineData("100", "TestUser", "TestEmail@email.com", "", "Testi")]
        public async Task CreateLocallyAsync_WhenNameIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(m => m[It.IsAny<string>()]).Returns("https://ids.ziralink.local:5001");

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("name", exception.ParamName);
        }
        
        [Theory]
        [InlineData("100", "TestUser", "TestEmail@email.com", "Test", "")]
        public async Task CreateLocallyAsync_WhenFamilyIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(m => m[It.IsAny<string>()]).Returns("https://ids.ziralink.local:5001");

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("family", exception.ParamName);
        }

        #endregion

    }
}
