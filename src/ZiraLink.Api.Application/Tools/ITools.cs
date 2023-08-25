using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZiraLink.Api.Application.Tools
{
    public interface ITools
    {
        Task<bool> CheckDomainExists(string domainUrl);
    }
}
