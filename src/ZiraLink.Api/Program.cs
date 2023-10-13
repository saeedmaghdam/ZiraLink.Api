using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZiraLink.Api;
using ZiraLink.Api.Application;
using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Framework;
using ZiraLink.Api.HostingExtensions;

var builder = WebApplication.CreateBuilder(args);

var pathToExe = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);

IConfiguration Configuration = new ConfigurationBuilder()
    .SetBasePath(pathToExe)
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
builder.Host.UseSerilog();

builder.Services.Register(Configuration, pathToExe);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
    // Only loopback proxies are allowed by default.
    // Clear that restriction because forwarders are enabled by explicit
    // configuration.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
context.Database.Migrate();

await app.InitializeTestEnvironmentAsync(Configuration);

if (string.IsNullOrWhiteSpace(Configuration["ZIRALINK_USE_HTTP"]) || !bool.Parse(Configuration["ZIRALINK_USE_HTTP"]!))
{
    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";
        return next(context);
    });
}
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseErrorHandler();

app.UseCors("AllowSpecificOrigins");

app.UseDefaultFiles();
app.UseStaticFiles();

if (string.IsNullOrWhiteSpace(Configuration["ZIRALINK_USE_HTTP"]) || !bool.Parse(Configuration["ZIRALINK_USE_HTTP"]!))
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseBff();

app.UseEndpoints(endpoints =>
{
    endpoints.MapBffManagementEndpoints();
    endpoints.MapControllers();

    endpoints.MapGet("/", async (HttpContext context, IHttpContextAccessor httpContextAccessor, ITokenService tokenService, ISessionService sessionService, ICustomerService customerService, CancellationToken cancellationToken) =>
    {
        var token = default(string);

        token = httpContextAccessor.HttpContext!.Request.Query["access_token"];
        if (string.IsNullOrEmpty(token))
            token = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");
        var idToken = await httpContextAccessor.HttpContext.GetTokenAsync("id_token");
        var refreshToken = await httpContextAccessor.HttpContext.GetTokenAsync("refresh_token");

        if (string.IsNullOrEmpty(token))
        {
            httpContextAccessor.HttpContext.Response.Redirect($"/swagger", true);
            return Task.FromResult(0);
        }


        var jwtSecurityToken = tokenService.ParseToken(token);
        var sub = jwtSecurityToken.Claims.Single(claim => claim.Type == "sub").Value;
        var tokenp = await tokenService.GenerateToken(sub, string.Empty, string.Empty);

        app.Logger.LogInformation($"{sub} logged in, Token: {token}, TokenP: {tokenp}");

        await tokenService.SetTokenPSubAsync(tokenp, sub);
        await tokenService.SetSubTokenAsync(sub, token);
        await tokenService.SetSubTokenPAsync(sub, tokenp);
        await tokenService.SetTokenPTokenAsync(tokenp, token);
        await tokenService.SetSubIdTokenAsync(sub, idToken!);
        await tokenService.SetTokenPRefreshTokenAsync(tokenp, refreshToken!);

        try
        {
            var _ = await customerService.GetCustomerByExternalIdAsync(sub, cancellationToken);
        }
        catch (NotFoundException)
        {
            var customerProfile = await sessionService.GetCurrentCustomerProfile(cancellationToken);
            var _ = await customerService.CreateLocallyAsync(sub, customerProfile.Username, customerProfile.Email, customerProfile.Name, customerProfile.Family, cancellationToken);
        }

        var baseUri = new Uri(Configuration["ZIRALINK_REDIRECTURI"]!);
        var uri = new Uri(baseUri, $"?access_token={tokenp}");
        httpContextAccessor.HttpContext.Response.Redirect(uri.ToString(), true);
        return Task.FromResult(0);
    }).ExcludeFromDescription();
});

app.Run();
