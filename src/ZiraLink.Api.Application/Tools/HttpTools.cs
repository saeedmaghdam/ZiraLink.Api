using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

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
                    var stream = await response.Content.ReadAsStreamAsync();
                } 
            }
            catch (Exception)
            { 
            }

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
                    var response = await Task.Run(() => ping.Send(domainUrl, 1200));
                    if (response.Status == IPStatus.Success)
                    {
                        return false;
                    }
                } 
            }
            catch (Exception)
            {
            }

            return true;
        } 
    }
}
