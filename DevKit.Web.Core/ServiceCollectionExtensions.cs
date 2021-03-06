using System;
using System.Linq;
using System.Reflection;
using System.Text;
using DevKit.Web.DependencyLifecycle;
using DevKit.Web.Services;
using DevKit.Web.Services.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DevKit.Web.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDevKitServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetAssembly(typeof(IService));
            var types = assembly.GetExportedTypes().Where(t =>
                t.IsClass && t.IsPublic && !t.IsAbstract && t.IsSubclassOf(typeof(IService)));

            foreach (Type type in types)
            {
                foreach (var @interface in type.GetInterfaces())
                {
                    switch (@interface)
                    {
                        case IDependencySingleton:
                            services.AddSingleton(@interface, type);
                            break;
                        case IDependencyScoped:
                            services.AddScoped(@interface, type);
                            break;
                        case IDependencyTransient:
                            services.AddTransient(@interface, type);
                            break;
                        default:
                            throw new NotImplementedException(
                                $"{@interface} service does not Implement DependencyLifecycle interfaces ");
                    }
                }
            }

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSetting jwtSettings)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var secretKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
                var encryptionKey = Encoding.UTF8.GetBytes(jwtSettings.EncryptionKey);

                var validationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),

                    RequireExpirationTime = true,
                    ValidateLifetime = true,

                    ValidateAudience = true, //default : false
                    ValidAudience = jwtSettings.Audience,

                    ValidateIssuer = true, //default : false
                    ValidIssuer = jwtSettings.Issuer,

                    TokenDecryptionKey = new SymmetricSecurityKey(encryptionKey)
                };

                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = validationParameters;
                //options.Events = new JwtBearerEvents
                //{
                //    OnAuthenticationFailed = context =>
                //    {
                //        //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                //        //logger.LogError("Authentication failed.", context.Exception);

                //        if (context.Exception != null)
                //            throw new AppException(ApiResultStatusCode.UnAuthorized, "Authentication failed.", HttpStatusCode.Unauthorized, context.Exception, null);

                //        return Task.CompletedTask;
                //    },
                //    OnTokenValidated = async context =>
                //    {
                //        var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
                //        var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

                //        var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                //        if (claimsIdentity.Claims?.Any() != true)
                //            context.Fail("This token has no claims.");

                //        var securityStamp = claimsIdentity.FindFirstValue(new ClaimsIdentityOptions().SecurityStampClaimType);
                //        if (!securityStamp.HasValue())
                //            context.Fail("This token has no security stamp");

                //        //Find user and token from database and perform your custom validation
                //        var userId = claimsIdentity.GetUserId<int>();
                //        var user = await userRepository.GetByIdAsync(context.HttpContext.RequestAborted, userId);

                //        //if (user.SecurityStamp != Guid.Parse(securityStamp))
                //        //    context.Fail("Token security stamp is not valid.");

                //        var validatedUser = await signInManager.ValidateSecurityStampAsync(context.Principal);
                //        if (validatedUser == null)
                //            context.Fail("Token security stamp is not valid.");

                //        if (!user.IsActive)
                //            context.Fail("User is not active.");

                //        await userRepository.UpdateLastLoginDateAsync(user, context.HttpContext.RequestAborted);
                //    },
                //    OnChallenge = context =>
                //    {
                //        //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                //        //logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);

                //        if (context.AuthenticateFailure != null)
                //            throw new AppException(ApiResultStatusCode.UnAuthorized, "Authenticate failure.", HttpStatusCode.Unauthorized, context.AuthenticateFailure, null);
                //        throw new AppException(ApiResultStatusCode.UnAuthorized, "You are unauthorized to access this resource.", HttpStatusCode.Unauthorized);

                //        //return Task.CompletedTask;
                //    }
                //};
            });

            return services;
        }

        public static IServiceCollection AddIdentity<TUser, TRole, TContext>(this IServiceCollection services, IdentitySettings identitySettings)
            where TContext : DbContext where TUser : class where TRole : class
        {
            services.AddIdentity<TUser, TRole>(options =>
                {
                    //Password Settings
                    options.Password.RequireDigit = identitySettings.PasswordRequireDigit;
                    options.Password.RequiredLength = identitySettings.PasswordRequiredLength;
                    options.Password.RequireNonAlphanumeric = identitySettings.PasswordRequireNonAlphanumeric; //#@!
                    options.Password.RequireUppercase = identitySettings.PasswordRequireUppercase;
                    options.Password.RequireLowercase = identitySettings.PasswordRequireLowercase;

                    //UserName Settings
                    options.User.RequireUniqueEmail = identitySettings.RequireUniqueEmail;

                    //Singin Settings
                    //options.SignIn.RequireConfirmedEmail = false;
                    //options.SignIn.RequireConfirmedPhoneNumber = false;

                    //Lockout Settings
                    //options.Lockout.MaxFailedAccessAttempts = 5;
                    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    //options.Lockout.AllowedForNewUsers = false;
                }).AddEntityFrameworkStores<TContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}