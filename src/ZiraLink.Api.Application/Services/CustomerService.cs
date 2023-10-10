using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Domain;
using IdentityModel.Client;
using ZiraLink.Api.Application.Framework;
using Microsoft.Extensions.Configuration;
using ZiraLink.Api.Application.Tools;

namespace ZiraLink.Api.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpTools _httpTools;

        public CustomerService(AppDbContext dbContext, IHttpTools httpTools)
        {
            _dbContext = dbContext;
            _httpTools = httpTools;
        }

        public async Task<Customer> GetCustomerByExternalIdAsync(string externalId, CancellationToken cancellationToken)
        {
            var customer = await _dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken);
            if (customer == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), externalId) });

            return customer;
        }

        public async Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Customers.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Guid> CreateLocallyAsync(string externalId, string username, string email, string name, string family, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            //if (string.IsNullOrEmpty(email))
            //    throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(family))
                throw new ArgumentNullException(nameof(family));

            var customer = new Customer
            {
                ViewId = Guid.NewGuid(),
                Username = username,
                Email = email,
                Name = name,
                Family = family,
                ExternalId = externalId
            };
            await _dbContext.Customers.AddAsync(customer, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return customer.ViewId;
        }

        public async Task<Guid> CreateAsync(string username, string password, string email, string name, string family, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(family))
                throw new ArgumentNullException(nameof(family));

            var customer = await _dbContext.Customers.SingleOrDefaultAsync(x => x.Username == username || x.Email == email, cancellationToken);
            if (customer != null) throw new ApplicationException("Customer exists");

            var httpClient = await _httpTools.InitializeHttpClientAsync(cancellationToken);

            var jsonObject = new
            {
                Username = username,
                Password = password,
                Email = email,
                Name = name,
                Family = family
            };

            // var content = new StringContent(JsonSerializer.Serialize(jsonObject), Encoding.UTF8, "application/json");
            // var baseUri = _idsUri;
            // var uri = new Uri(baseUri, "User");
            // var response = await client.PostAsync(uri.ToString(), content);

            // var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            // var userCreationResult = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var userCreationResult = await _httpTools.CallIDSApis<ApiResponse<string>>(httpClient, "User", jsonObject, HttpMethod.Post, cancellationToken);
            if (!userCreationResult.Status)
                throw new ApplicationException("User creation on identity server failed");


            customer = new Customer
            {
                ViewId = Guid.NewGuid(),
                Username = username,
                Email = email,
                Name = name,
                Family = family,
                ExternalId = userCreationResult!.Data!
            };
            await _dbContext.Customers.AddAsync(customer, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return customer.ViewId;
        }

        public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(currentPassword))
                throw new ArgumentNullException(nameof(currentPassword));
            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentNullException(nameof(newPassword));

            var customer = await _dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.ExternalId == userId, cancellationToken);
            if (customer == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), userId) });

            var httpClient = await _httpTools.InitializeHttpClientAsync(cancellationToken);

            var jsonObject = new
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };
            // var content = new StringContent(JsonSerializer.Serialize(jsonObject), Encoding.UTF8, "application/json");
            // var baseUri = _idsUri;
            // var uri = new Uri(baseUri, "User/ChangePassword");
            // var response = await client.PatchAsync(uri.ToString(), content);

            // var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            // var userCreationResult = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var userCreationResult = await _httpTools.CallIDSApis<ApiResponse<string>>(httpClient, "User/ChangePassword", jsonObject, HttpMethod.Patch, cancellationToken);
            if (!userCreationResult.Status)
                throw new ApplicationException("User creation on identity server failed");
        }

        public async Task UpdateProfileAsync(string userId, string name, string family, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(family))
                throw new ArgumentNullException(nameof(family));

            var customer = await _dbContext.Customers.SingleOrDefaultAsync(x => x.ExternalId == userId, cancellationToken);
            if (customer == null)
                throw new NotFoundException(nameof(Customer), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Customer.ExternalId), userId) });

            var httpClient = await _httpTools.InitializeHttpClientAsync(cancellationToken);

            var jsonObject = new
            {
                UserId = userId,
                Name = name,
                Family = family
            };
            // var content = new StringContent(JsonSerializer.Serialize(jsonObject), Encoding.UTF8, "application/json");
            // var baseUri = _idsUri;
            // var uri = new Uri(baseUri, "User");
            // var response = await client.PatchAsync(uri.ToString(), content);

            // var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            // var userCreationResult = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var userCreationResult = await _httpTools.CallIDSApis<ApiResponse<string>>(httpClient, "User", jsonObject, HttpMethod.Post, cancellationToken);
            if (!userCreationResult.Status)
                throw new ApplicationException("User creation on identity server failed");

            customer.Name = name;
            customer.Family = family;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        protected int Count { get; set; }
    }
}
