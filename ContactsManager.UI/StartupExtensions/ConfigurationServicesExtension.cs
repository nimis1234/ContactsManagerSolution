using ContactManager.Core.Domain.IdentityEntity;
using ContactManager.Core.Interfaces;
using ContactManager.Core.Services;
using CURDExample.Filters;
using Entities;
using Interfaces;
using IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using Repository;
using Services;

namespace CURDExample.StartUpExtensions
{
    public static class ConfigurationServicesExtension //synatx namewith extension
    {
        // its a extension method which is used to configure the services which will registered in program .cs
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {

            // adding services
            services.AddControllersWithViews(
                 //enable anti forger token globally

                 options=>options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())

                );

            services.AddScoped<IcountryServices, CountryServices>();
            services.AddScoped<IpersonServices, PersonServices>();

            services.AddScoped<ICountryRepo, CountryRepo>();
            services.AddScoped<IPersonRepo, PersonRepo>();

            services.AddScoped<ModalValidationFilter>();
            services.AddScoped<ICustomEmailService, CustomEmailService>();

            services.AddScoped<IAccountServices, AccountServices>();
            // Register HTTP Logging service with some options
            services.AddHttpLogging(options =>
            {
                options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
            });


            // database connection
            if (!environment.IsEnvironment("IntegrationTest"))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                });
            }


            //enable identity service 

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    //  need email confirmation
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.SignIn.RequireConfirmedAccount = true; // Email confirmation required
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
            //now add the repositories , here repository are added virtaully in background using userstore
               .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
               .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();
            // Configure Authentication // fall back policy is used to enforce authorization policy for all the action methods
            /*
             
             Configure the FallbackPolicy in AddAuthorization to require authentication for
             all actions (like the example you provided), you don't need to add the [Authorize] attribute to every action. This is because the FallbackPolicy is applied globally
            , meaning any request will automatically require authentication
            unless a specific policy is applied to a given action.
             */

            services.AddAuthorization(options =>
                {
                    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); //enforces authoriation policy (user must be authenticated) for all the action methods
                
                    //  we can add our own custom policies here
                    options.AddPolicy("Unauthenticated",
                        policy=> policy.RequireAssertion(context => !context.User.Identity.IsAuthenticated)); //RequireAssertion is used to create a custom policy that checks if the user is not authenticated
                });

                services.ConfigureApplicationCookie(options => {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(120); // Expire after 5 minutes
                    options.SlidingExpiration = true; // Reset expiration if user is active
                    options.LoginPath = "/Account/Login";
                });



            // Add CORS policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin() // Allow requests from any origin
                               .AllowAnyMethod()  // Allow any HTTP method (GET, POST, DELETE, etc.)
                               .AllowAnyHeader(); // Allow any headers
                    });
            });


            //To run the seeding logic during the application startup, use a IHostedService 
            // Call the role and user seeding method during app initialization
            services.AddTransient<IHostedService, StartupSeedService>();


            return services;
        }
    }
}
