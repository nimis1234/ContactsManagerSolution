
using ContactManager.Core.Domain.IdentityEntity;
using Microsoft.AspNetCore.Identity;

namespace ContactsManager.UI.StartupExtensions.RolesSeed
{
    public static class SeedRoles
    {

        public  static async Task AddRoles(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            // define roles
            string[] roleNames = { "Admin", "User", "Manager" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var newRole = new ApplicationRole { Name = roleName,Description=roleName };
                    await roleManager.CreateAsync(newRole);
                }
            }

            // create a super user who could maintain the web app
            var adminEmail = "admin@admin.com";
            var adminPassword = "Admin@12345";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null) {

                ApplicationUser user = new ApplicationUser()
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "Admin",
                    ProfilePicture = null,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }

    }
}
