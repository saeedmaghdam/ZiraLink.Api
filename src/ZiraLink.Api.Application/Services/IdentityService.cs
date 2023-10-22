using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using IdentityModel.Client;

using Microsoft.Extensions.Configuration;

using ZiraLink.Api.Application.Enums;
using ZiraLink.Api.Application.Framework;

namespace ZiraLink.Api.Application.Services
{

    public class IdentityService : IIdentityService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly Uri _idsUri; 
        public IdentityService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            _idsUri = new Uri(configuration["ZIRALINK_URL_IDS"]!); 
        }
         

        public async Task<HttpClient> InitializeHttpClientAsync(CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient(NamedHttpClients.Default);
            var disco = await httpClient.GetDiscoveryDocumentAsync(_idsUri.ToString(), cancellationToken);
            if (disco.IsError)
                throw new ApplicationException("Failed to get discivery document");

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "back",
                ClientSecret = "secret",
                Scope = "ziralink IdentityServerApi"
            }, cancellationToken);

            if (tokenResponse.IsError)
                throw new ApplicationException("Failed to get token from identity server");

            httpClient.SetBearerToken(tokenResponse.AccessToken);

            return httpClient;
        }

        public async Task<ApiResponse<string>> CreateUserAsync(object jsonObject, CancellationToken cancellationToken)
        {
            var httpClient = await InitializeHttpClientAsync(cancellationToken);
            var content = new StringContent(JsonSerializer.Serialize(jsonObject), Encoding.UTF8, "application/json");
            var baseUri = _idsUri;
            var uri = new Uri(baseUri, "User");

            HttpResponseMessage? response;

            response = await httpClient.PostAsync(uri.ToString(), content);
            ApiResponse<string> userCreationResult;
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            userCreationResult = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new ApiResponse<string>();

            return userCreationResult;
        }
        
        public async Task<ApiResponse<string>> ChangePasswordAsync(object jsonObject, CancellationToken cancellationToken)
        {
            var httpClient = await InitializeHttpClientAsync(cancellationToken);
            var content = new StringContent(JsonSerializer.Serialize(jsonObject), Encoding.UTF8, "application/json");
            var baseUri = _idsUri;
            var uri = new Uri(baseUri, "User/ChangePassword");

            HttpResponseMessage? response;

            response = await httpClient.PatchAsync(uri.ToString(), content);
            ApiResponse<string> userCreationResult;
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            userCreationResult = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new ApiResponse<string>();

            return userCreationResult;
        }
        
        public async Task<ApiResponse<string>> UpdateUserAsync(object jsonObject, CancellationToken cancellationToken)
        {
            var httpClient = await InitializeHttpClientAsync(cancellationToken);
            var content = new StringContent(JsonSerializer.Serialize(jsonObject), Encoding.UTF8, "application/json");
            var baseUri = _idsUri;
            var uri = new Uri(baseUri, "User");

            HttpResponseMessage? response;

            response = await httpClient.PatchAsync(uri.ToString(), content);
            ApiResponse<string> userCreationResult;
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            userCreationResult = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new ApiResponse<string>();

            return userCreationResult;
        }
         
    }
}
