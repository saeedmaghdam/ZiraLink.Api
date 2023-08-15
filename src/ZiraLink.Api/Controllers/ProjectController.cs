using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiraLink.Api.Application;
using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Api.Application.Framework;
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

        [HttpGet]
        public async Task<ApiResponse<List<Project>>> GetAsync(CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            var result = await _projectService.GetAsync(cancellationToken);
            return ApiResponse<List<Project>>.CreateSuccessResponse(result);
        }

        [HttpPost]
        public async Task<ApiResponse<Guid>> CreateAsync([FromBody] CreateProjectInputModel model, CancellationToken cancellationToken)
        {
            var customer = await _sessionService.GetCurrentCustomer(cancellationToken);
            if (customer == null)
                throw new NotFoundException("Customer");

            var result = await _projectService.CreateAsync(customer.Id, model.Title, model.DomainType, model.Domain, model.InternalUrl, cancellationToken);
            return ApiResponse<Guid>.CreateSuccessResponse(result);
        }
    }
}
