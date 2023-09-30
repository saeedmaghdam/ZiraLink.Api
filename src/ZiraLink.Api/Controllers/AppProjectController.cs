using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Api.Application.Framework;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Framework;
using ZiraLink.Api.Models.AppProject.InputModels;
using ZiraLink.Api.Models.Project.InputModels;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    public class AppProjectController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly IAppProjectService _appProjectService;

        public AppProjectController(IAppProjectService appProjectService, ISessionService sessionService)
        {
            _sessionService = sessionService;
            _appProjectService = appProjectService;
        }

        /// <summary>
        /// Returns list of all app projects for the current user
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpGet("{appProjectType}")]
        public async Task<ApiResponse<List<AppProject>>> GetAsync([FromRoute] AppProjectType appProjectType, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            var result = await _appProjectService.GetAsync(customer.Id, appProjectType, cancellationToken);
            return ApiResponse<List<AppProject>>.CreateSuccessResponse(result);
        }

        /// <summary>
        /// Returns list of all app projects
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("All")]
        public async Task<ApiResponse<List<AppProject>>> GetAllAsync(CancellationToken cancellationToken)
        {
            var result = await _appProjectService.GetAllAsync(cancellationToken);
            return ApiResponse<List<AppProject>>.CreateSuccessResponse(result);
        }
         
        /// <summary>
        /// Returns a single app projects
        /// </summary>
        /// <param name="id">Project Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpGet("GetById/{id}")]
        public async Task<ApiResponse<AppProject>> GetByIdAsync([FromRoute] long id, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            var result = await _appProjectService.GetByIdAsync(id, customer.Id, cancellationToken);
            return ApiResponse<AppProject>.CreateSuccessResponse(result);
        }

        /// <summary>
        /// Creates a new app projects
        /// </summary>
        /// <param name="model">CreateProjectInputModel type</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpPost]
        public async Task<ApiResponse<Guid>> CreateAsync([FromBody] CreateAppProjectInputModel model, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            var result = await _appProjectService.CreateAsync(customer.Id, model.Title, model.AppProjectViewId, model.AppProjectType, model.PortType, model.InternalPort, model.State, cancellationToken);
            return ApiResponse<Guid>.CreateSuccessResponse(result);
        }

        /// <summary>
        /// Deletes a app project
        /// </summary>
        /// <param name="id">App Project Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpDelete("{id}")]
        public async Task<ApiDefaultResponse> DeleteAsync([FromRoute] long id, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            await _appProjectService.DeleteAsync(customer.Id, id, cancellationToken);
            return ApiDefaultResponse.CreateSuccessResponse();
        }

        /// <summary>
        /// Updates a app project
        /// </summary>
        /// <param name="id">App Project Id</param>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpPatch("{id}")]
        public async Task<ApiDefaultResponse> PatchAsync([FromRoute] long id, [FromBody] PatchAppProjectInputModel model, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            await _appProjectService.PatchAsync(id, customer.Id, model.Title, model.AppProjectViewId, model.AppProjectType, model.PortType, model.InternalPort, model.State, cancellationToken);

            return ApiDefaultResponse.CreateSuccessResponse();
        }
    }
}
