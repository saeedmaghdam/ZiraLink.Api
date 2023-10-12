using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Duende.Bff.Yarp;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using ZiraLink.Api.Application;
using ZiraLink.Api.Application.Enums;
using ZiraLink.Api.Application.Services;
using ZiraLink.Api.Application.Tools;
using ZiraLink.Api.Framework;

namespace ZiraLink.Api
{
    public static class DependencyResolver
    {
        private static void PrintAllOptions(IConfiguration configuration, IEnumerable<IConfigurationSection> sections, string parentKey)
        {
            foreach (var section in sections)
            {
                string currentKey = !string.IsNullOrEmpty(parentKey) ? $"{parentKey}:{section.Key}" : section.Key;
                string value = section.Value;

                if (section.GetChildren().Any())
                {
                    // If this is a section, recursively print its children
                    PrintAllOptions(configuration, section.GetChildren(), currentKey);
                }
                else
                {
                    // If it's a leaf node, print the key and value
                    Log.Information($"{currentKey}: {value}");
                }
            }
        }

        public static void Register(this IServiceCollection services, IConfiguration configuration, string pathToExe)
        {
            PrintAllOptions(configuration, configuration.GetChildren(), string.Empty);

            IdentityModelEventSource.ShowPII = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (configuration["ASPNETCORE_ENVIRONMENT"] == "Test")
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    string expectedThumbprint = configuration["ZIRALINK_CERT_THUMBPRINT_LOCALHOST"]!;
                    if (certificate!.GetCertHashString() == expectedThumbprint)
                        return true;

                    if (sslPolicyErrors == SslPolicyErrors.None)
                        return true;

                    return false;
                };

                services.AddHttpClient(NamedHttpClients.Default).ConfigurePrimaryHttpMessageHandler(_ =>
                {
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        string expectedThumbprint = configuration["ZIRALINK_CERT_THUMBPRINT_LOCALHOST"]!;
                        if (certificate!.GetCertHashString() == expectedThumbprint)
                            return true;

                        if (sslPolicyErrors == SslPolicyErrors.None)
                            return true;

                        return false;
                    };
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12;
                    handler.ClientCertificates.Add(new X509Certificate2(Path.Combine(pathToExe, "certs", "s3d-localhost-server.pfx"), configuration["ASPNETCORE_Kestrel__Certificates__Default__Password"]!));

                    return handler;
                });
            }
            else
            {
                services.AddHttpClient(NamedHttpClients.Default);
            }

            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IAppProjectService, AppProjectService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IBus, Bus>();
            services.AddSingleton<IHttpTools, HttpTools>();
            // Add services to the container.
            services.AddCors(options =>
            {
                var uri = new Uri(configuration["ZIRALINK_WEB_URL"]!);
                var webUrl = $"{uri.Scheme}://{uri.Authority}";
                options.AddPolicy("AllowSpecificOrigins",
                    builder =>
                    {
                        builder.WithOrigins(webUrl)
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
                            if (!string.IsNullOrWhiteSpace(configuration["ZIRALINK_USE_HTTP"]) && bool.Parse(configuration["ZIRALINK_USE_HTTP"]!))
                            {
                                options.RequireHttpsMetadata = false;
                            }

                            options.Authority = new Uri(configuration["ZIRALINK_URL_IDS"]!).ToString();
                            options.SaveToken = true;
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateAudience = false
                            };

                            if (configuration["ASPNETCORE_ENVIRONMENT"] == "Test")
                            {
                                var handler = new HttpClientHandler();
                                handler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                                {
                                    string expectedThumbprint = configuration["ZIRALINK_CERT_THUMBPRINT_LOCALHOST"]!;
                                    if (certificate!.GetCertHashString() == expectedThumbprint)
                                        return true;

                                    if (sslPolicyErrors == SslPolicyErrors.None)
                                        return true;

                                    return false;
                                };
                                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                                handler.SslProtocols = SslProtocols.Tls12;
                                handler.ClientCertificates.Add(new X509Certificate2(Path.Combine(pathToExe, "certs", "s3d-localhost-server.pfx"), configuration["ASPNETCORE_Kestrel__Certificates__Default__Password"]!));

                                options.BackchannelHttpHandler = handler;
                            }

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
                            if (!string.IsNullOrWhiteSpace(configuration["ZIRALINK_USE_HTTP"]) && bool.Parse(configuration["ZIRALINK_USE_HTTP"]!))
                            {
                                options.RequireHttpsMetadata = false;
                            }

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
                            options.Scope.Add("offline_access");
                            options.SaveTokens = true;

                            options.GetClaimsFromUserInfoEndpoint = true;

                            if (configuration["ASPNETCORE_ENVIRONMENT"] == "Test")
                            {
                                var handler = new HttpClientHandler();
                                handler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                                {
                                    string expectedThumbprint = configuration["ZIRALINK_CERT_THUMBPRINT_LOCALHOST"]!;
                                    if (certificate!.GetCertHashString() == expectedThumbprint)
                                        return true;

                                    if (sslPolicyErrors == SslPolicyErrors.None)
                                        return true;

                                    return false;
                                };
                                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                                handler.SslProtocols = SslProtocols.Tls12;
                                handler.ClientCertificates.Add(new X509Certificate2(Path.Combine(pathToExe, "certs", "s3d-localhost-server.pfx"), configuration["ASPNETCORE_Kestrel__Certificates__Default__Password"]!));

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
        }
    }
}
