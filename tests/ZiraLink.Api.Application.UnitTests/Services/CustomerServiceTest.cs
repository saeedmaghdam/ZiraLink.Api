
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
            var mockHttpTools = new Mock<IHttpTools>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

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
            var mockHttpTools = new Mock<IHttpTools>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => customerService.GetCustomerByExternalIdAsync(externalId, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetAll_ShouldHasData()
        {
            //Arrange
            var mockHttpTools = new Mock<IHttpTools>();

            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var response = await customerService.GetAllAsync(CancellationToken.None);
            Assert.True(response is List<Customer>);
            Assert.True(response.Any());
        }
        #endregion

        #region CreateCustomerLocally
        [Theory]
        [InlineData("10", "TestUser", "TestEmail@email.com", "Test", "Testi")]
        public async Task CreateCustomerLocally_WhenEverythingIsOk_ShouldBeSucceeded(string externalId, string username, string email, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

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
        public async Task CreateCustomerLocally_WhenUsernameIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("username", exception.ParamName);
        }

        [Theory]
        [InlineData("10", "TestUser", "TestEmail@email.com", "", "Testi")]
        public async Task CreateCustomerLocally_WhenNameIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("name", exception.ParamName);
        }

        [Theory]
        [InlineData("100", "TestUser", "TestEmail@email.com", "Test", "")]
        public async Task CreateCustomerLocally_WhenFamilyIsEmpty_ShouldBeFailed(string externalId, string username, string email, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateLocallyAsync(externalId, username, email, name, family, CancellationToken.None));
            Assert.Equal("family", exception.ParamName);
        }

        #endregion

        #region CreateCustomer
        [Theory]
        [InlineData("TestUser", "TestPassword", "TestEmail@email.com", "Test", "Testi")]
        public async Task CreateCustomer_WhenEverythingIsOk_ShouldBeSucceeded(string username, string password, string email, string name, string family)
        {
            var mockHttpClient = new Mock<HttpClient>(); 
            var mockHttpTools = new Mock<IHttpTools>();
       
            var idsData = new ApiResponse<string> { Data = "10", Status = true };
 
            mockHttpTools.Setup(p => p.InitializeHttpClientAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockHttpClient.Object);
            mockHttpTools.Setup(p => p.CallIDSApis<ApiResponse<string>>(mockHttpClient.Object, It.IsAny<string>(), It.IsAny<object>(), It.IsAny<HttpMethod>(), It.IsAny<CancellationToken>())).ReturnsAsync(idsData);

            var customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

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
        [InlineData("", "TestPassword", "TestEmail@email.com", "Test", "Testi")]
        public async Task CreateCustomer_WhenUsernameIsEmpty_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            Assert.True(string.IsNullOrWhiteSpace(username));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("username", exception.ParamName);
        }

        [Theory]
        [InlineData("TestUser", "", "TestEmail@email.com", "Test", "Testi")]
        public async Task CreateCustomer_WhenPasswordIsEmpty_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            Assert.True(string.IsNullOrWhiteSpace(password));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("password", exception.ParamName);
        }

        [Theory]
        [InlineData("TestUser", "TestPassword", "", "Test", "Testi")]
        public async Task CreateCustomer_WhenEmailIsEmpty_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            Assert.True(string.IsNullOrWhiteSpace(email));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("email", exception.ParamName);
        }

        [Theory]
        [InlineData("TestUser", "TestPassword", "TestEmail@email.com", "", "Testi")]
        public async Task CreateCustomer_WhenNameIsEmpty_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            Assert.True(string.IsNullOrWhiteSpace(name));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("name", exception.ParamName);
        }

        [Theory]
        [InlineData("TestUser", "TestPassword", "TestEmail@email.com", "Test", "")]
        public async Task CreateCustomer_WhenFamilyIsEmpty_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            Assert.True(string.IsNullOrWhiteSpace(family));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("family", exception.ParamName);
        }

        [Theory]
        [InlineData("TestUser2", "TestPassword", "TestUser2@ZiraLink.com", "Test", "Testi")]
        public async Task CreateCustomer_WhenCustomerExists_ShouldBeFailed(string username, string password, string email, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ApplicationException>(() => customerService.CreateAsync(username, password, email, name, family, CancellationToken.None));
            Assert.Equal("Customer exists", exception.Message);
        }

        #endregion

        #region ChangePassword
        [Theory]
        [InlineData("3", "TestPassword", "TestNewPassword")]
        public async Task ChangePassword_WhenEverythingIsOk_ShouldBeSucceeded(string userId, string currentPassword, string newPassword)
        {
            var mockHttpClient = new Mock<HttpClient>(); 
            var mockHttpTools = new Mock<IHttpTools>();
       
            var idsData = new ApiResponse<string> { Data = "10", Status = true };
 
            mockHttpTools.Setup(p => p.InitializeHttpClientAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockHttpClient.Object);
            mockHttpTools.Setup(p => p.CallIDSApis<ApiResponse<string>>(mockHttpClient.Object, It.IsAny<string>(), It.IsAny<object>(), It.IsAny<HttpMethod>(), It.IsAny<CancellationToken>())).ReturnsAsync(idsData);

            var customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            await customerService.ChangePasswordAsync(userId, currentPassword, newPassword, CancellationToken.None);

            Assert.True(true);
        }

        [Theory]
        [InlineData("", "TestPassword", "TestNewPassword")]
        public async Task ChangePassword_WhenUserIdIsEmpty_ShouldBeFailed(string userId, string currentPassword, string newPassword)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            Assert.True(string.IsNullOrWhiteSpace(userId));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.ChangePasswordAsync(userId, currentPassword, newPassword, CancellationToken.None));
            Assert.Equal("userId", exception.ParamName);
        }

        [Theory]
        [InlineData("3", "", "TestNewPassword")]
        public async Task ChangePassword_WhenCurrentPasswordIsEmpty_ShouldBeFailed(string userId, string currentPassword, string newPassword)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            Assert.True(string.IsNullOrWhiteSpace(currentPassword));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.ChangePasswordAsync(userId, currentPassword, newPassword, CancellationToken.None));
            Assert.Equal("currentPassword", exception.ParamName);
        }

        [Theory]
        [InlineData("3", "TestPassword", "")]
        public async Task ChangePassword_WhenNewPasswordIsEmpty_ShouldBeFailed(string userId, string currentPassword, string newPassword)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            Assert.True(string.IsNullOrWhiteSpace(newPassword));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.ChangePasswordAsync(userId, currentPassword, newPassword, CancellationToken.None));
            Assert.Equal("newPassword", exception.ParamName);
        }

        [Theory]
        [InlineData("9999", "TestPassword", "TestNewPassword")]
        public async Task ChangePassword_WhenCustomerIdIsNotExist_ShouldBeFailed(string userId, string currentPassword, string newPassword)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => customerService.ChangePasswordAsync(userId, currentPassword, newPassword, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }

        #endregion

        #region UpdateProfile
        [Theory]
        [InlineData("3", "Test", "Testi")]
        public async Task UpdateProfile_WhenEverythingIsOk_ShouldBeSucceeded(string userId, string name, string family)
        {
            var mockHttpClient = new Mock<HttpClient>(); 
            var mockHttpTools = new Mock<IHttpTools>();
       
            var idsData = new ApiResponse<string> { Data = "10", Status = true };
 
            mockHttpTools.Setup(p => p.InitializeHttpClientAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockHttpClient.Object);
            mockHttpTools.Setup(p => p.CallIDSApis<ApiResponse<string>>(mockHttpClient.Object, It.IsAny<string>(), It.IsAny<object>(), It.IsAny<HttpMethod>(), It.IsAny<CancellationToken>())).ReturnsAsync(idsData);

            var customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);
 
            await customerService.UpdateProfileAsync(userId, name, family, CancellationToken.None);

            Assert.True(true); 
        }

        [Theory]
        [InlineData("", "Test", "Testi")]
        public async Task UpdateProfile_WhenUserIdIsEmpty_ShouldBeFailed(string userId, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            Assert.True(string.IsNullOrWhiteSpace(userId));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.UpdateProfileAsync(userId, name, family, CancellationToken.None));
            Assert.Equal("userId", exception.ParamName);
        }

        [Theory]
        [InlineData("3", "", "Testi")]
        public async Task UpdateProfile_WhenNameIsEmpty_ShouldBeFailed(string userId, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            Assert.True(string.IsNullOrWhiteSpace(name));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.UpdateProfileAsync(userId, name, family, CancellationToken.None));
            Assert.Equal("name", exception.ParamName);
        }

        [Theory]
        [InlineData("3", "Test", "")]
        public async Task UpdateProfile_WhenFamilyIsEmpty_ShouldBeFailed(string userId, string name, string family)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            Assert.True(string.IsNullOrWhiteSpace(family));
            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => customerService.UpdateProfileAsync(userId, name, family, CancellationToken.None));
            Assert.Equal("family", exception.ParamName);
        }

        [Theory]
        [InlineData("999", "Test", "Testi")]
        public async Task UpdateProfiled_WhenCustomerIdIsNotExist_ShouldBeFailed(string userId, string currentPassword, string newPassword)
        {
            var mockHttpTools = new Mock<IHttpTools>();


            CustomerService customerService = new CustomerService(_testTools.AppMemoryDbContext, mockHttpTools.Object);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => customerService.UpdateProfileAsync(userId, currentPassword, newPassword, CancellationToken.None));
            Assert.Equal("Customer", exception.Message);
        }

        #endregion

    }
}
