
using System.Text.Json;
using IdentityModel.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Api.Application.Framework;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.UnitTests.Tools;
using ZiraLink.Domain;
using System.Net;
using ZiraLink.Api.Application.Tools;

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
            var mockIdentityService = new Mock<IIdentityService>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var response = await customerService.GetCustomerByExternalIdAsync(externalId, CancellationToken.None);
            Assert.True(response is Customer);
            Assert.Equal(externalId, response.ExternalId);
            Assert.Equal("TestUser1", response.Username);
            Assert.Equal("TestName1", response.Name);
            Assert.Equal("User1", response.Family);
            Assert.Equal("TestUser1@ziralink.local", response.Email);
        }

        [Theory]
        [InlineData("999")]
        public async Task GetCustomerByExternalId_WhenExternalIdIsNotExist_ShouldHasData(string externalId)
        {
            //Arrange
            var mockIdentityService = new Mock<IIdentityService>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => customerService.GetCustomerByExternalIdAsync(externalId, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetAll_ShouldHasData()
        {
            //Arrange
            var mockIdentityService = new Mock<IIdentityService>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var response = await customerService.GetAllAsync(CancellationToken.None);
            Assert.True(response is List<Customer>);
            Assert.True(response.Any());
        }
        #endregion

        #region CreateLocallyAsync
        [Theory]
        [InlineData("10", "TestUserLocally", "TestLocallyEmail@ziralink.local", "TestLocal", "TestLocally")]
        public async Task CreateLocallyAsync_WhenEverythingIsOk_ShouldBeSucceeded(string externalId, string username, string email, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

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
        [InlineData("10", "", "TestEmail@ziralink.local", "Test", "Testi")]
        public async Task CreateLocallyAsync_WhenUsernameIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("username", exception.ParamName);
        }

        [Theory]
        [InlineData("10", "TestUser", "TestEmail@ziralink.local", "", "Testi")]
        public async Task CreateLocallyAsync_WhenNameIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("name", exception.ParamName);
        }

        [Theory]
        [InlineData("100", "TestUser", "TestEmail@ziralink.local", "Test", "")]
        public async Task CreateLocallyAsync_WhenFamilyIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("family", exception.ParamName);
        }

        #endregion

        #region CreateCustomer
        [Theory]
        [InlineData("TestUser", "TestPassword", "TestEmail@ziralink.local", "Test", "Testi")]
        public async Task CreateAsync_WhenEverythingIsOk_ShouldBeSucceeded(string username, string password, string email, string name, string family)
        { 
            var mockIdentityService = new Mock<IIdentityService>();
       
            var idsData = new ApiResponse<string> { Data = "90", Status = true };
  
            mockIdentityService.Setup(p => p.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(idsData);

            var customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var response = await customerService.CreateAsync(username, password, email, name, family, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, response);

            var createdRow = await _testTools.AppMemoryDbContext.Customers.Where(x => x.ViewId == response).FirstOrDefaultAsync();
            Assert.NotNull(createdRow);

            Assert.Equal(idsData.Data, createdRow.ExternalId);
            Assert.Equal(username, createdRow.Username);
            Assert.Equal(email, createdRow.Email);
            Assert.Equal(name, createdRow.Name);
            Assert.Equal(family, createdRow.Family);
        }

        [Theory]
        [InlineData("", "TestPassword", "TestEmail@ziralink.local", "Test", "Testi")]
        public async Task CreateAsync_WhenUsernameIsEmpty_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();


            Assert.True(string.IsNullOrWhiteSpace(username));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("username", exception.ParamName);
        }

        [Theory]
        [InlineData("TestUser", "", "TestEmail@ziralink.local", "Test", "Testi")]
        public async Task CreateAsync_WhenPasswordIsEmpty_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();

            Assert.True(string.IsNullOrWhiteSpace(password));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("password", exception.ParamName);
        }

        [Theory]
        [InlineData("TestUser", "TestPassword", "", "Test", "Testi")]
        public async Task CreateAsync_WhenEmailIsEmpty_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();

            Assert.True(string.IsNullOrWhiteSpace(email));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("email", exception.ParamName);
        }

        [Theory]
        [InlineData("TestUser", "TestPassword", "TestEmail@ziralink.local", "", "Testi")]
        public async Task CreateAsync_WhenNameIsEmpty_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();

            Assert.True(string.IsNullOrWhiteSpace(name));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("name", exception.ParamName);
        }

        [Theory]
        [InlineData("TestUser", "TestPassword", "TestEmail@ziralink.local", "Test", "")]
        public async Task CreateAsync_WhenFamilyIsEmpty_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();

            Assert.True(string.IsNullOrWhiteSpace(family));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("family", exception.ParamName);
        }

        [Theory]
        [InlineData("TestUser2", "TestPassword", "TestUser2@ziralink.local", "Test", "Testi")]
        public async Task CreateAsync_WhenCustomerExists_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ApplicationException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("Customer exists", exception.Message);
        }

        #endregion

        #region ChangePasswordAsync
        [Theory]
        [InlineData("3", "TestPassword", "TestNewPassword")]
        public async Task ChangePasswordAsynWhenEverythingIsOk_ShouldBeSucceeded(string userId, string currentPassword, string newPassword)
        {
            var mockHttpClient = new Mock<HttpClient>(); 
            var mockIdentityService = new Mock<IIdentityService>();
       
            var idsData = new ApiResponse<string> { Data = "10", Status = true };

            mockIdentityService.Setup(p => p.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(idsData);

            var customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            await customerService.ChangePasswordAsync(userId, currentPassword, newPassword, CancellationToken.None);

            Assert.True(true);
        }

        [Theory]
        [InlineData("", "TestPassword", "TestNewPassword")]
        public async Task ChangePasswordAsynWhenUserIdIsEmpty_ShouldBeFailed(string userId, string currentPassword, string newPassword)
        {
            var mockIdentityService = new Mock<IIdentityService>();
             
            Assert.True(string.IsNullOrWhiteSpace(userId));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.ChangePasswordAsync(userId, currentPassword, newPassword, CancellationToken.None));
            Assert.Equal("userId", exception.ParamName);
        }

        [Theory]
        [InlineData("3", "", "TestNewPassword")]
        public async Task ChangePasswordAsynWhenCurrentPasswordIsEmpty_ShouldBeFailed(string userId, string currentPassword, string newPassword)
        {
            var mockIdentityService = new Mock<IIdentityService>();
             
            Assert.True(string.IsNullOrWhiteSpace(currentPassword));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.ChangePasswordAsync(userId, currentPassword, newPassword, CancellationToken.None));
            Assert.Equal("currentPassword", exception.ParamName);
        }

        [Theory]
        [InlineData("3", "TestPassword", "")]
        public async Task ChangePasswordAsynWhenNewPasswordIsEmpty_ShouldBeFailed(string userId, string currentPassword, string newPassword)
        {
            var mockIdentityService = new Mock<IIdentityService>();
             
            Assert.True(string.IsNullOrWhiteSpace(newPassword));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.ChangePasswordAsync(userId, currentPassword, newPassword, CancellationToken.None));
            Assert.Equal("newPassword", exception.ParamName);
        }

        [Theory]
        [InlineData("9999", "TestPassword", "TestNewPassword")]
        public async Task ChangePasswordAsynWhenCustomerIdIsNotExist_ShouldBeFailed(string userId, string currentPassword, string newPassword)
        {
            var mockIdentityService = new Mock<IIdentityService>();
             
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => customerService.ChangePasswordAsync(userId, currentPassword, newPassword, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }

        #endregion

        #region UpdateProfileAsync
        [Theory]
        [InlineData("3", "Test", "Testi")]
        public async Task UpdateProfileAsync_WhenEverythingIsOk_ShouldBeSucceeded(string userId, string name, string family)
        {
            var mockHttpClient = new Mock<HttpClient>(); 
            var mockIdentityService = new Mock<IIdentityService>();
       
            var idsData = new ApiResponse<string> { Data = "10", Status = true };
             
            mockIdentityService.Setup(p => p.UpdateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(idsData);

            var customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);
 
            await customerService.UpdateProfileAsync(userId, name, family, CancellationToken.None);

            Assert.True(true); 
        }

        [Theory]
        [InlineData("", "Test", "Testi")]
        public async Task UpdateProfileAsync_WhenUserIdIsEmpty_ShouldBeFailed(string userId, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();
             
            Assert.True(string.IsNullOrWhiteSpace(userId));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.UpdateProfileAsync(userId, name, family, CancellationToken.None));
            Assert.Equal("userId", exception.ParamName);
        }

        [Theory]
        [InlineData("3", "", "Testi")]
        public async Task UpdateProfileAsync_WhenNameIsEmpty_ShouldBeFailed(string userId, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();
             
            Assert.True(string.IsNullOrWhiteSpace(name));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.UpdateProfileAsync(userId, name, family, CancellationToken.None));
            Assert.Equal("name", exception.ParamName);
        }

        [Theory]
        [InlineData("3", "Test", "")]
        public async Task UpdateProfileAsync_WhenFamilyIsEmpty_ShouldBeFailed(string userId, string name, string family)
        {
            var mockIdentityService = new Mock<IIdentityService>();
             
            Assert.True(string.IsNullOrWhiteSpace(family));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.UpdateProfileAsync(userId, name, family, CancellationToken.None));
            Assert.Equal("family", exception.ParamName);
        }

        [Theory]
        [InlineData("999", "Test", "Testi")]
        public async Task UpdateProfiled_WhenCustomerIdIsNotExist_ShouldBeFailed(string userId, string currentPassword, string newPassword)
        {
            var mockIdentityService = new Mock<IIdentityService>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockIdentityService.Object);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => customerService.UpdateProfileAsync(userId, currentPassword, newPassword, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }

        #endregion

    }
}
