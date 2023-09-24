using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ZiraLink.Api.Application.Exceptions;
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
        [InlineData("1")]
        public async Task GetCustomerByExternalId_WhenEverythingIsOk_ShouldHasData(string externalId)
        {
            //Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(m => m["ZIRALINK_URL_IDS"]).Returns("https://ids.ziralink.local:5001");

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var response = await customerService.GetCustomerByExternalIdAsync(externalId, CancellationToken.None);
            Assert.True(response is Customer);
            Assert.Equal(externalId, response.ExternalId); 
            Assert.Equal("TestUser1", response.Username);
            Assert.Equal("TestName1", response.Name);
            Assert.Equal("User1", response.Family);
            Assert.Equal("TestUser1@ZiraLink.com", response.Email);
        }

        [Theory]
        [InlineData("999")]
        public async Task GetCustomerByExternalId_WhenExternalIdIsNotExist_ShouldHasData(string externalId)
        {
            //Arrange
            var mockConfiguration = new Mock<IConfiguration>(); 
            mockConfiguration.Setup(m => m["ZIRALINK_URL_IDS"]).Returns("https://ids.ziralink.local:5001");

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => customerService.GetCustomerByExternalIdAsync(externalId, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }
        #endregion

        #region GetAll
        [Fact] 
        public async Task GetAll_ShouldHasData()
        {
            //Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(m => m["ZIRALINK_URL_IDS"]).Returns("https://ids.ziralink.local:5001");

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var response = await customerService.GetAllAsync(CancellationToken.None); 
            Assert.True(response is List<Customer>); 
            Assert.True(response.Any());
        } 
        #endregion

        #region CreateLocallyAsync
        [Theory]
        [InlineData("10", "TestUser", "TestEmail@email.com", "Test", "Testi")]
        public async Task CreateLocallyAsync_WhenEverythingIsOk_ShouldBeSucceeded(string externalId, string username, string email, string name, string family)
        {
            var mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(m => m["ZIRALINK_URL_IDS"]).Returns("https://ids.ziralink.local:5001");
            
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
        [InlineData("10", "", "TestEmail@email.com", "Test", "Testi")]
        public async Task CreateLocallyAsync_WhenUsernameIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(m => m["ZIRALINK_URL_IDS"]).Returns("https://ids.ziralink.local:5001");

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("username", exception.ParamName);
        }
        
        [Theory]
        [InlineData("10", "TestUser", "TestEmail@email.com", "", "Testi")]
        public async Task CreateLocallyAsync_WhenNameIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(m => m["ZIRALINK_URL_IDS"]).Returns("https://ids.ziralink.local:5001");

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("name", exception.ParamName);
        }
        
        [Theory]
        [InlineData("100", "TestUser", "TestEmail@email.com", "Test", "")]
        public async Task CreateLocallyAsync_WhenFamilyIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(m => m["ZIRALINK_URL_IDS"]).Returns("https://ids.ziralink.local:5001");

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("family", exception.ParamName);
        }

        #endregion

    }
}
