﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZiraLink.Api.Application.Tools
{
    public interface IHttpTools
    {
        Task<bool> CheckDomainExists(string domainUrl);
        Task<bool> PingDomain(string simpleDomain);
        Task<HttpClient> InitializeHttpClientAsync(CancellationToken cancellationToken);
        Task<T> CallIDSApis<T>(HttpClient httpClient, string relativeUri, object jsonObject, HttpMethod httpMethod, CancellationToken cancellationToken);
    }
}
