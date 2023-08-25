using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ZiraLink.Api.Application.Tools
{
    public class Tools : ITools
    {
        public async Task<bool> CheckDomainExists(string domainUrl)
        {
            HttpClient _client = new HttpClient();
            try
            {
                HttpResponseMessage response = await _client.GetAsync(domainUrl);
                var bb = response.Content.ReadAsStringAsync();
                return false;
            }
            catch (Exception)
            {
            }

            try
            { 
                string simpleDomain = domainUrl.Replace("https://", "").Replace("http://", "");
                Ping ping = new Ping();
                for (int i = 0; i < 2; i++)
                {
                    var response = ping.Send(simpleDomain, 120);
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
