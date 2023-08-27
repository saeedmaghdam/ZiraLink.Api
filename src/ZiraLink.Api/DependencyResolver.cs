using System.IdentityModel.Tokens.Jwt;
using Duende.Bff.Yarp;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using ZiraLink.Api.Application;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.Tools;
using ZiraLink.Api.Framework;
namespace ZiraLink.Api;

public static class DependencyResolver
{
    public static void Register(this IServiceCollection services, IConfiguration configuration, string pathToExe)
    {
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IBus, Bus>();
        services.AddSingleton<IHttpTools, HttpTools>();
        // Add services to the container.
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                builder =>
                {
                    builder.WithOrigins(new Uri(configuration["ZIRALINK_WEB_URL"]!).ToString())
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .Build();
                });
        });

        var connectionString = configuration["ASPNETCORE_ENVIRONMENT"] == "Development" ? $"Data Source={Path.Combine(pathToExe, "database.db")}" : configuration["ZIRALINK_CONNECTIONSTRINGS_DB"];
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        //services.AddDbContext<AppDbContext>();

        var connectionMultiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { configuration["ZIRALINK_CONNECTIONSTRINGS_REDIS"]! },
            AbortOnConnectFail = false,
            Password = configuration["ZIRALINK_REDIS_PASSWORD"]!
        });
        services.AddSingleton<IConnectionMultiplexer>(sp => connectionMultiplexer);

        services.AddAuthorization();
        services
            .AddBff()
            .AddRemoteApis();
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = "oidc";
            options.DefaultSignOutScheme = "oidc";
        })
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddJwtBearer("Bearer", options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.Authority = new Uri(configuration["ZIRALINK_URL_IDS"]!).ToString();
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
                        options.Authority = new Uri(configuration["ZIRALINK_URL_IDS"]!).ToString();
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

                        if (configuration["ASPNETCORE_ENVIRONMENT"] != "Production")
                        {
                            HttpClientHandler handler = new HttpClientHandler();
                            options.BackchannelHttpHandler = handler;
                        }
                    });

        services.AddHttpContextAccessor();

        services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(setup =>
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

        services.AddHttpClient();

    }
}
