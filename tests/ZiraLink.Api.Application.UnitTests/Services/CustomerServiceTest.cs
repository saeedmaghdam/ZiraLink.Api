
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

        #region CreateAsync
        [Theory]
        [InlineData("TestUser", "TestPassword", "TestEmail@email.com", "Test", "Testi")]
        public async Task CreateAsync_WhenEverythingIsOk_ShouldBeSucceeded(string username, string password, string email, string name, string family)
        {
            var mockConfiguration = new Mock<IConfiguration>();
            var mockhttpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockhttpMessageHandlerMock.Object);

            mockConfiguration.Setup(m => m["ZIRALINK_URL_IDS"]).Returns("https://ids.ziralink.local:5001");

            //var successResponse = new ApiResponse<string> { Status = true, Data = "someId" };

            // var discoveryDocumentContent = "{\"token_endpoint\": \"some_endpoint\", \"IsError\": false}";
            // var tokenResponseContent = "{\"access_token\": \"some_access_token\", \"expires_in\": 3600, \"token_type\": \"Bearer\"}";
            // var userCreationResponseContent = "{\"status\": true, \"data\": \"some_external_id\"}";

            // mockhttpMessageHandlerMock.Protected()
            //     .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            //     .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(discoveryDocumentContent) })
            //     .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(tokenResponseContent) })
            //     .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(userCreationResponseContent) });


            //var customerService = new CustomerService(_testTools.AppMemoryDbContext, mockConfiguration.Object);
            var customerService = new Mock<CustomerService>(_testTools.AppMemoryDbContext, mockConfiguration.Object);

            customerService.Protected().SetupGet<HttpClient>("InitializeHttpClientAsync").Returns(mockHttpClient);
            

            var response = await customerService.Object.CreateAsync(username, password, email, name, family, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, response);

            var createdRow = await _testTools.AppMemoryDbContext.Customers.Where(x => x.ViewId == response).FirstOrDefaultAsync();
            Assert.NotNull(createdRow);

            Assert.Equal(username, createdRow.Username);
            Assert.Equal(email, createdRow.Email);
            Assert.Equal(name, createdRow.Name);
            Assert.Equal(family, createdRow.Family);
        }

        #endregion
    }
}
