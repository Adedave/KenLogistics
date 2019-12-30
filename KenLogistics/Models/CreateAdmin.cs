using KenLogistics.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KenLogistics.Web
{
    public class CreateAdmin
    {
        public static async Task CreateAdminAccount(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            UserManager<ApplicationUser> userManager =
            serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            RoleManager<UserRole> roleManager =
            serviceProvider.GetRequiredService<RoleManager<UserRole>>();
            string username = configuration["Data:AdminUser:Name"];
            string email = configuration["Data:AdminUser:Email"];
            string password = configuration["Data:AdminUser:Password"];
            string role = configuration["Data:AdminUser:Role"];

            if (await userManager.FindByNameAsync(username) == null)
            {
                if (await roleManager.FindByNameAsync(role) == null)
                {
                    await roleManager.CreateAsync(new UserRole { Name = role});
                    await roleManager.CreateAsync(new UserRole { Name = "User" });
                }
                ApplicationUser user = new ApplicationUser
                {
                    UserName = username,
                    Email = email
                };
                IdentityResult result = await userManager
                .CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
