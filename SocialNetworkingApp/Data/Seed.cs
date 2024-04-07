using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Data
{
    public class Seed
    {
        public static async Task SeedUsersAndRolesAsync(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();

                // Ensure the database is created
                dbContext.Database.EnsureCreated();

                if (!await roleManager.RoleExistsAsync(UserRoles.User))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

                // Users
                string appUserEmail = "user@example.com";
                var appuser = await userManager.FindByEmailAsync(appUserEmail);

                if (appuser == null)
                {
                    var newAppUser = new User()
                    {
                        UserName = "user1",
                        Email = appUserEmail,
                        EmailConfirmed = true,
                        FirstName = "TEST",
                        LastName = "TEST"
                    };

                    await userManager.CreateAsync(newAppUser, "252609Mm!");
                    await userManager.AddToRoleAsync(newAppUser, UserRoles.User);
                }
            }
        }
    }
}
