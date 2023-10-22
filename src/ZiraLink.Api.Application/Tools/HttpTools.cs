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


    }
}
