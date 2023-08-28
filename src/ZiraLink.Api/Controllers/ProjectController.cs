using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Api.Application.Framework;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Framework;
using ZiraLink.Api.Models.Project.InputModels;
using ZiraLink.Domain;

namespace ZiraLink.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService, ISessionService sessionService)
        {
            _sessionService = sessionService;
            _projectService = projectService;
        }

        /// <summary>
        /// Return all lists of projects for every user exceptionally
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpGet]
        public async Task<ApiResponse<List<Project>>> GetAsync(CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            var result = await _projectService.GetAsync(customer.Id, cancellationToken);
            return ApiResponse<List<Project>>.CreateSuccessResponse(result);
        }

        /// <summary>
        /// Return all list of projects 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("All")]
        public async Task<ApiResponse<List<Project>>> GetAllAsync(CancellationToken cancellationToken)
        {
            var result = await _projectService.GetAllAsync(cancellationToken);
            return ApiResponse<List<Project>>.CreateSuccessResponse(result);
        }


        /// <summary>
        /// Return a project queried with ID
        /// </summary>
        /// <param name="id">Project Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpGet("{id}")]
        public async Task<ApiResponse<Project>> GetByIdAsync([FromRoute] long id, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            var result = await _projectService.GetByIdAsync(id, customer.Id, cancellationToken);
            return ApiResponse<Project>.CreateSuccessResponse(result);
        }

        /// <summary>
        /// Create new project for every user
        /// </summary>
        /// <param name="model">CreateProjectInputModel type</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpPost]
        public async Task<ApiResponse<Guid>> CreateAsync([FromBody] CreateProjectInputModel model, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            var result = await _projectService.CreateAsync(customer.Id, model.Title, model.DomainType, model.Domain, model.InternalUrl, model.State, cancellationToken);
            return ApiResponse<Guid>.CreateSuccessResponse(result);
        }

        /// <summary>
        /// Delete a project
        /// </summary>
        /// <param name="id">Project Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpDelete("{id}")]
        public async Task<ApiDefaultResponse> DeleteAsync([FromRoute] long id, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            await _projectService.DeleteAsync(customer.Id, id, cancellationToken);
            return ApiDefaultResponse.CreateSuccessResponse();
        }

        /// <summary>
        /// Update a project
        /// </summary>
        /// <param name="id">Project Id</param>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        [HttpPatch("{id}")]
        public async Task<ApiDefaultResponse> PatchAsync([FromRoute] long id, [FromBody] PatchProjectInputModel model, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            await _projectService.PatchAsync(id, customer.Id, model.Title, model.DomainType, model.Domain, model.InternalUrl, model.State, cancellationToken);

            return ApiDefaultResponse.CreateSuccessResponse();
        }
    }
}
