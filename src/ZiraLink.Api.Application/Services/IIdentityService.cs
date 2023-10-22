using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZiraLink.Api.Application.Framework;

namespace ZiraLink.Api.Application.Services
{
    public interface IIdentityService
    {
        Task<HttpClient> InitializeHttpClientAsync(CancellationToken cancellationToken);
        Task<ApiResponse<string>> CreateUserAsync(object jsonObject, CancellationToken cancellationToken);
        Task<ApiResponse<string>> ChangePasswordAsync(object jsonObject, CancellationToken cancellationToken);
        Task<ApiResponse<string>> UpdateUserAsync(object jsonObject, CancellationToken cancellationToken);

    }
}
