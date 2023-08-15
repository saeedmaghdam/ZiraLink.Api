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
using ZiraLink.Api.Application;
using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Domain;

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

        [HttpGet("Profile")]
        public async Task<ApiResponse<Customer>> GetProfileAsync(CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            return ApiResponse<Customer>.CreateSuccessResponse(customer);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ApiResponse<Guid>> CreateAsync([FromBody] CreateCustomerInputModel model, CancellationToken cancellationToken)
        {
            var result = await _customerService.CreateAsync(model.Username, model.Password, model.Email, model.Name, model.Family, cancellationToken);

            return ApiResponse<Guid>.CreateSuccessResponse(result);
        }

        [HttpPatch("ChangePassword")]
        public async Task<ApiDefaultResponse> ChangePasswordAsync([FromBody] ChangePasswordInputModel model, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            await _customerService.ChangePasswordAsync(customer.ExternalId, model.CurrentPassword, model.NewPassword, cancellationToken);

            return ApiDefaultResponse.CreateSuccessResponse();
        }

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
