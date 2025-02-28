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
                if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

                // Users
                string userName = "user1";
                var appuser = await userManager.FindByNameAsync(userName);

                if (appuser == null)
                {
                    var newAppUser = new User()
                    {
                        UserName = userName,
                        EmailConfirmed = true,
                        FirstName = "TEST",
                        LastName = "TEST"
                    };

                    await userManager.CreateAsync(newAppUser, "252609Mm!");
                    await userManager.AddToRoleAsync(newAppUser, UserRoles.User);

                }

                string userAdmin = "admin";
                var admin = await userManager.FindByNameAsync(userAdmin);

                if (admin == null)
                {
                    var newAdmin = new User()
                    {
                        UserName = userAdmin,
                        EmailConfirmed = true,
                        FirstName = "admin",
                        LastName = "admin"
                    };

                    await userManager.CreateAsync(newAdmin, "252609Mm!");
                    await userManager.AddToRoleAsync(newAdmin, UserRoles.Admin);

                }

                if (dbContext.ProjectTypes.Count() == 0)
                {
                    dbContext.Add(new ProjectType { Type = "Учебный проект" });
                    dbContext.Add(new ProjectType { Type = "Стартап" });
                    dbContext.Add(new ProjectType { Type = "Научная работа" });
                }

                if (dbContext.PostTypes.Count() == 0)
                {
                    dbContext.Add(new PostType { Type = "Профиль" });
                    dbContext.Add(new PostType { Type = "Сообщество" });
                    dbContext.Add(new PostType { Type = "Проект" });
                }

                if (dbContext.ProjectStatuses.Count() == 0)
                {
                    dbContext.Add(new ProjectStatus { Status = "Зарождение" });
                    dbContext.Add(new ProjectStatus { Status = "В разработке" });
                    dbContext.Add(new ProjectStatus { Status = "Приостановлен" });
                    dbContext.Add(new ProjectStatus { Status = "Завершен" });
                }

                if (dbContext.StudentGroups.Count() == 0)
                {
                    dbContext.Add(new StudentGroup { GroupName = "РИС-22-1" });
                    dbContext.Add(new StudentGroup { GroupName = "РИС-22-2" });
                    dbContext.Add(new StudentGroup { GroupName = "РИС-22-3" });
                }
                
                dbContext.SaveChanges();
            }
        }
    }
}
