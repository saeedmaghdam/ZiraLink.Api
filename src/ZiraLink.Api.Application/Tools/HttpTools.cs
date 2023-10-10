using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using ZiraLink.Api.Application.Enums;
using ZiraLink.Api.Application.Framework;

namespace ZiraLink.Api.Application.Tools
{
    public class HttpTools : IHttpTools
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly Uri _idsUri;
        public HttpTools(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            _idsUri = new Uri(configuration["ZIRALINK_URL_IDS"]!);
        }

        public async Task<bool> CheckDomainExists(string domainUrl)
        {
            var httpClient = _httpClientFactory.CreateClient(NamedHttpClients.Default);
            try
            {
                using (var response = await httpClient.GetAsync(domainUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    var urlData = await response.Content.ReadAsStringAsync();
                    return false;
                }
            }
            catch { }

            return await PingDomain(domainUrl);

        }

        public async Task<bool> PingDomain(string domainUrl)
        {
            domainUrl = domainUrl.Replace("https://", "").Replace("http://", "");
            try
            {
                Ping ping = new Ping();
                for (int i = 0; i < 2; i++)
                {
                    var response = await ping.SendPingAsync(domainUrl, 1200);
                    if (response.Status == IPStatus.Success)
                    {
                        return false;
                    }
                }
            }
            catch { }

            return true;
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

        public async Task<T> CallIDSApis<T>(HttpClient httpClient, string relativeUri, object jsonObject, HttpMethod httpMethod, CancellationToken cancellationToken)
        { 
            var content = new StringContent(JsonSerializer.Serialize(jsonObject), Encoding.UTF8, "application/json");
            var baseUri = _idsUri;
            var uri = new Uri(baseUri, relativeUri);

            HttpResponseMessage? response;

            if (httpMethod.Equals(HttpMethod.Post)) response = await httpClient.PostAsync(uri.ToString(), content);
            else if (httpMethod.Equals(HttpMethod.Patch)) response = await httpClient.PatchAsync(uri.ToString(), content);
            else if (httpMethod.Equals(HttpMethod.Get)) response = await httpClient.GetAsync(uri.ToString());
            else if (httpMethod.Equals(HttpMethod.Delete)) response = await httpClient.DeleteAsync(uri.ToString());
            else if (httpMethod.Equals(HttpMethod.Put)) response = await httpClient.PutAsync(uri.ToString(), content);
            else response = await httpClient.GetAsync(uri.ToString());
            
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var userCreationResult = JsonSerializer.Deserialize<T>(responseString, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return userCreationResult;
        }


    }
}
