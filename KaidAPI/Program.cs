using System.Security.Claims;
using KaidAPI.Context;
using KaidAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace KaidAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        string? connectionString = builder.Configuration.GetConnectionString("MySql");
        builder.Services.AddDbContext<ServerDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        builder.Services.AddControllers();
        builder.Services.AddScoped<IKaidUserService, KaidUserService>();
        
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                var oidcConfig = builder.Configuration.GetSection("OpenIDConnectSettings");
                options.RequireHttpsMetadata = true;
                
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                options.Authority = oidcConfig["Authority"];
                options.ClientId = oidcConfig["ClientId"];
                options.ClientSecret = oidcConfig["ClientSecret"];

                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = OpenIdConnectResponseType.Code;
                
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.SaveTokens = true;
                
                options.CallbackPath = "/signin-oidc"; 
                options.SignedOutCallbackPath = "/signout-callback-oidc"; 
                options.GetClaimsFromUserInfoEndpoint = true;

                options.MapInboundClaims = true;
                options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                options.TokenValidationParameters.RoleClaimType = "roles";

                options.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userService = context.HttpContext.RequestServices.GetRequiredService<IKaidUserService>();
                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("OidcEvents.OnTokenValidated");
                        var issuer = context.Principal?.FindFirstValue("iss");
                        var subject =
                            context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var email = context.Principal?.FindFirstValue(ClaimTypes.Email);
                        var name = context.Principal?.FindFirstValue("preferred_username") ?? "Unknown";

                        if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(subject))
                        {
                            logger.LogError("Issuer or Subject claim is missing in OnTokenValidated.");
                            context.Fail("Required identity claims are missing.");
                            return;
                        }

                        try
                        {
                            Guid localUserId =
                                await userService.FindOrCreateUserByOidcAsync(issuer, subject, email, name);
                            logger.LogInformation("User found or created with local Kaid UserID: {UserId}",
                                localUserId);
                            
                            var claims = new List<Claim>
                            {
                                new Claim("kaid_user_id", localUserId.ToString()),
                                
                                new Claim(ClaimTypes.NameIdentifier, subject),
                                new Claim(ClaimTypes.Email, email ?? ""),
                                new Claim(ClaimTypes.Name, name ?? "")
                            };

                            var claimsIdentity = new ClaimsIdentity(claims,
                                CookieAuthenticationDefaults.AuthenticationScheme);
                            var localClaimsPrincipal = new ClaimsPrincipal(claimsIdentity);


                            await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                localClaimsPrincipal);


                            logger.LogInformation("Local session created successfully for Kaid UserID: {UserId}",
                                localUserId);

                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex,
                                "Error processing OIDC token validation for issuer {Issuer}, subject {Subject}.",
                                issuer, subject);
                            context.Fail("An error occurred while processing your sign-in.");
                        }
                    }
                };
            });
        
        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {

        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapGet("/", () => "Hello! This is the public homepage.").RequireAuthorization();

        app.Run();
    }
}