using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KaidAPI.Context;
using KaidAPI.Repositories;
using KaidAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KaidAPI;

public class Program
{
    public static void Main(string[] args)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        var builder = WebApplication.CreateBuilder(args);
        string? connectionString = builder.Configuration.GetConnectionString("MySql");
        var oidcConfig = builder.Configuration.GetSection("OpenIDConnectSettings"); 

        builder.Services.AddDbContext<ServerDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        
        builder.Services.AddControllers();
        builder.Services.AddScoped<IKaidUserService, KaidUserService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
        builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => 
            {
                options.Authority = oidcConfig["Authority"];

                options.Audience = oidcConfig["ClientId"]; 

                if (builder.Environment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false; 
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = 
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                }
                else
                {
                    options.RequireHttpsMetadata = true; 
                }


                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true, 
                    ValidateIssuerSigningKey = true, 
                    NameClaimType = "sub"
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context => 
                    {
                        
                        var userService = context.HttpContext.RequestServices.GetRequiredService<IKaidUserService>();
                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("JwtBearerEvents.OnTokenValidated");

                        var principal = context.Principal;
                        var issuer = principal?.FindFirstValue("iss");
                        var subject = principal?.FindFirstValue(ClaimTypes.NameIdentifier); 
                        if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(subject))
                        {
                            logger.LogWarning("Issuer or Subject claim is missing in JWT OnTokenValidated.");
                            context.Fail("Required identity claims are missing in JWT."); 
                            return;
                        }
                        
                        var email = principal?.FindFirstValue(ClaimTypes.Email);
                        var name = principal?.FindFirstValue("preferred_username") 
                                   ?? principal?.FindFirstValue(ClaimTypes.Name) 
                                   ?? "Unknown";
                        try
                        {
                            Guid localUserId = await userService.FindOrCreateUserByOidcAsync(issuer, subject, email, name);
                            logger.LogInformation("User mapping for JWT: Kaid UserId {UserId} for subject {Subject}", localUserId, subject);

                            var claimsIdentity = principal.Identity as ClaimsIdentity;
                            if (claimsIdentity != null && !claimsIdentity.HasClaim(c => c.Type == "kaid_user_id"))
                            {
                                claimsIdentity.AddClaim(new Claim("kaid_user_id", localUserId.ToString()));
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error processing JWT token validation for local user mapping. Issuer: {Issuer}, Subject: {Subject}", issuer, subject);
                            context.Fail("An error occurred while processing your JWT.");
                        }
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("JwtBearerEvents.OnAuthenticationFailed");
                        logger.LogError(context.Exception, "JWT Authentication Failed.");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("JwtBearerEvents.OnChallenge");
                        logger.LogWarning("JWT Authentication challenge triggered. Path: {Path}", context.Request.Path);
                        return Task.CompletedTask;
                    }
                };
            });
        
        builder.Services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme) 
                .RequireAuthenticatedUser()
                .Build();
        });
            
        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error"); 
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication(); 
        app.UseAuthorization(); 
        
        app.MapControllers().RequireAuthorization(); 

        app.Run();
    }
}
