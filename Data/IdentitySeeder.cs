using Microsoft.AspNetCore.Identity;
using NLI_POS.Models;

namespace NLI_POS.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!dbContext.OfficeCountry.Any(o => o.Id == 1))
            {
                dbContext.OfficeCountry.Add(new OfficeCountry
                {
                    Id = 1,
                    Name = "Manila HQ",
                    OffCode="MHQ",
                    isActive = true,
                    CountryId=176,
                    EncodeDate = DateTime.UtcNow.AddHours(8),
                    EncodedBy = "System"
                });
                await dbContext.SaveChangesAsync();
            }


            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "CS", "Accounting","Inventory" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Optional: Create default admin user
            var adminEmail = "admin@noblelifeintl.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Admin",
                    Designation = "Administrator",
                    OfficeId = 1
                };

                var result = await userManager.CreateAsync(user, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
