using System.Net.NetworkInformation;

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
            var httpClient = _httpClientFactory.CreateClient();
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
    }
}
