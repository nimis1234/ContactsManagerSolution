
using ContactManager.Core.Domain.IdentityEntity;
using ContactsManager.UI.StartupExtensions.RolesSeed;
using Microsoft.AspNetCore.Identity;

namespace CURDExample.StartUpExtensions
{
    public class StartupSeedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public StartupSeedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async  Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope()) // creating a scope of work
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                await SeedRoles.AddRoles(_serviceProvider, userManager, roleManager);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}