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
        Task<ApiResponse<string>> CreateUserAsync(string username, string password, string email, string name, string family, CancellationToken cancellationToken);
        Task<ApiResponse<string>> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
        Task<ApiResponse<string>> UpdateUserAsync(string userId, string name, string family, CancellationToken cancellationToken);

    }
}
