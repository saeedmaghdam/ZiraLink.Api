using System.Net.NetworkInformation;
using IdentityModel.Client;
using ZiraLink.Api.Application.Enums;

namespace ZiraLink.Api.Application.Tools
{
    public class HttpTools : IHttpTools
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public HttpTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

        
        public async Task<HttpClient> InitializeHttpClientAsync(string idsUri, CancellationToken cancellationToken)
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(idsUri.ToString(), cancellationToken);
            if (disco.IsError)
                throw new ApplicationException("Failed to get discivery document");

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "back",
                ClientSecret = "secret",
                Scope = "ziralink IdentityServerApi"
            }, cancellationToken);

            if (tokenResponse.IsError)
                throw new ApplicationException("Failed to get token from identity server");

            client.SetBearerToken(tokenResponse.AccessToken);

            return client;
        }


    }
}
