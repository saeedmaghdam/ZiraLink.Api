using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using Duende.Bff.Yarp;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using ZiraLink.Api;
using ZiraLink.Api.Application;
using ZiraLink.Api.Application.Exceptions;
using ZiraLink.Api.Framework;

var builder = WebApplication.CreateBuilder(args);

var pathToExe = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
Directory.SetCurrentDirectory(pathToExe!);

IConfiguration Configuration = new ConfigurationBuilder()
    .SetBasePath(pathToExe)
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables()
    .Build();

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:3001", "https://localhost:3001", "http://ziralink.com:3000", "https://ziralink.com:3001", "http://ziralink.com:4000", "https://ziralink.com:4001")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .Build();
        });
});

var connectionString = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? $"Data Source={Path.Combine(pathToExe, "database.db")}" : Environment.GetEnvironmentVariable("ZIRALINK_CONNECTIONSTRINGS_DB");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
//builder.Services.AddDbContext<AppDbContext>();

var connectionMultiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { Configuration["ZIRALINK_CONNECTIONSTRINGS_REDIS"]! },
    AbortOnConnectFail = false,
    Password = Configuration["ZIRALINK_REDIS_PASSWORD"]!
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => connectionMultiplexer);

builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddAuthorization();
builder.Services
    .AddBff()
    .AddRemoteApis();
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddAuthentication(options =>
{
    //options.DefaultScheme = "Cookies";
    //options.DefaultChallengeScheme = "oidc";
    //options.DefaultSignOutScheme = "oidc";
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "oidc";
    options.DefaultSignOutScheme = "oidc";
})
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = false;
                options.Authority = Configuration["ZIRALINK_URL_IDS"]!;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false
                };

                var db = connectionMultiplexer.GetDatabase(10);

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = async context =>
                    {
                        var tokenp = context.Request.Headers["Authorization"];
                        tokenp = tokenp.ToString().Replace("Bearer ", "");
                        var token = await db.StringGetAsync($"tokenptoken:{tokenp}");
                        context.Token = token;
                    }
                };
            })
            .AddOpenIdConnect("oidc", options =>
            {
                options.RequireHttpsMetadata = false;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = Configuration["ZIRALINK_URL_IDS"]!;
                options.ClientId = "bff";
                options.ClientSecret = "secret";
                options.ResponseType = OidcConstants.ResponseTypes.Code;
                options.Scope.Clear();
                options.Scope.Add("ziralink");
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.SaveTokens = true;

                options.GetClaimsFromUserInfoEndpoint = true;

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
                {
                    HttpClientHandler handler = new HttpClientHandler();
                    //handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    options.BackchannelHttpHandler = handler;
                }
            });

builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

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

app.Use((context, next) =>
{
    context.Request.Scheme = "https";
    return next(context);
});
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseErrorHandler();

app.UseCors("AllowSpecificOrigins");

app.UseDefaultFiles();
app.UseStaticFiles();

//app.UseHttpsRedirection();

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

        token = httpContextAccessor.HttpContext.Request.Query["access_token"];
        if (string.IsNullOrEmpty(token))
            token = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");
        var id_token = await httpContextAccessor.HttpContext.GetTokenAsync("id_token");

        if (string.IsNullOrEmpty(token))
        {
            httpContextAccessor.HttpContext.Response.Redirect($"/swagger", true);
            return Task.FromResult(0);
        }


        var jwtSecurityToken = tokenService.ParseToken(token);
        var sub = jwtSecurityToken.Claims.Single(claim => claim.Type == "sub").Value;
        var tokenp = await tokenService.GenerateToken(sub, String.Empty, String.Empty);

        app.Logger.LogInformation($"{sub} logged in, Token: {token}, TokenP: {tokenp}");

        await tokenService.SetTokenPSubAsync(tokenp, sub);
        await tokenService.SetSubTokenAsync(sub, token);
        await tokenService.SetSubTokenPAsync(sub, tokenp);
        await tokenService.SetTokenPTokenAsync(tokenp, token);
        await tokenService.SetSubIdTokenAsync(sub, id_token);

        try
        {
            var _ = await customerService.GetCustomerByExternalIdAsync(sub, cancellationToken);
        }
        catch (NotFoundException)
        {
            var customerProfile = await sessionService.GetCurrentCustomerProfile(cancellationToken);
            var _ = await customerService.CreateLocallyAsync(sub, customerProfile.Username, customerProfile.Email, customerProfile.Name, customerProfile.Family, cancellationToken);
        }

        var uri = new Uri(Configuration["ZIRALINK_REDIRECTURI"]!);
        httpContextAccessor.HttpContext.Response.Redirect($"{uri}?access_token={tokenp}", true);
        return Task.FromResult(0);
    }).ExcludeFromDescription();
});

app.Run();
