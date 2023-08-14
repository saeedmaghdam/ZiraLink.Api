using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace ZiraLink.Api.Framework
{
    public static class HostingExtensions
    {
        public static void UseErrorHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerPathFeature>();
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(ApiResponse<object>.CreateFailureResponse($"{exception.Error.GetType().Name}: {exception.Error.Message}", 9001), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                });
            });
        }
    }
}
