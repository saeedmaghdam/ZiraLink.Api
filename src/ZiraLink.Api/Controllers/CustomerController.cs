using System.Text.Json.Nodes;
using System.Text;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiraLink.Api.Framework;
using ZiraLink.Api.Models;
using System.Text.Json;
using ZiraLink.Api.Models.Customer;
using ZiraLink.Api.Application.Framework;
using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Domain;
using ZiraLink.Api.Application.Services;

namespace ZiraLink.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    public class CustomerController
    {

        private readonly ISessionService _sessionService;
        private readonly ICustomerService _customerService;

        public CustomerController(ISessionService sessionService, ICustomerService customerService)
        {
            _sessionService = sessionService;
            _customerService = customerService;
        }

        /// <summary>
        /// Return a user profile
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpGet("Profile")]
        public async Task<ApiResponse<Customer>> GetProfileAsync(CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            return ApiResponse<Customer>.CreateSuccessResponse(customer);
        }

        /// <summary>
        /// Create a neew user
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ApiResponse<Guid>> CreateAsync([FromBody] CreateCustomerInputModel model, CancellationToken cancellationToken)
        {
            var result = await _customerService.CreateAsync(model.Username, model.Password, model.Email, model.Name, model.Family, cancellationToken);

            return ApiResponse<Guid>.CreateSuccessResponse(result);
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpPatch("ChangePassword")]
        public async Task<ApiDefaultResponse> ChangePasswordAsync([FromBody] ChangePasswordInputModel model, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            await _customerService.ChangePasswordAsync(customer.ExternalId, model.CurrentPassword, model.NewPassword, cancellationToken);

            return ApiDefaultResponse.CreateSuccessResponse();
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpPatch]
        public async Task<ApiDefaultResponse> UpdateProfileAsync([FromBody] UpdateProfileInputModel model, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            await _customerService.UpdateProfileAsync(customer.ExternalId, model.Name, model.Family, cancellationToken);

            return ApiDefaultResponse.CreateSuccessResponse();
        }
    }
}
