using IdentityMail.Web.Context;
using IdentityMail.Web.Entites;
using IdentityMail.Web.Entites.Enums;
using Microsoft.AspNetCore.Identity;

namespace IdentityMail.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            string[] roles = new[] { "Süper Admin", "Admin", "Standart Kullanıcı" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new AppRole { Name = roleName });
                }
            }

            var adminEmail = "auth@cybermail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new AppUser
                {
                    FirstName = "Cyber",
                    LastName = "Admin",
                    UserName = "auth",
                    Email = adminEmail,
                    ProfileImageUrl = "/images/avatars/default-avatar.jpg",
                    EmailConfirmed = true
                };

                // ŞİFRE
                var result = await userManager.CreateAsync(newAdmin, "Password12*");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Süper Admin");
                }
            }

            if (!context.MessageCategories.Any())
            {
                var categories = new List<MessageCategory>
                {
                    new MessageCategory
                    {
                        Name = "Genel",
                        Icon = "all_inbox",
                        Theme = CategoryTheme.PurpleTheme,
                        Description = "Herhangi bir Nitelik Taşımayan Genel Mesajlar",
                        IsSystemProtected = true
                    },
                    new MessageCategory
                    {
                        Name = "Sosyal / Topluluk",
                        Icon = "forum",
                        Theme = CategoryTheme.BlueTheme,
                        Description = "Sosyal Medya ve Topluluk Bildirimleri",
                        IsSystemProtected = true
                    },
                    new MessageCategory
                    {
                        Name = "Promosyon / Duyuru",
                        Icon = "local_offer",
                        Theme = CategoryTheme.YellowTheme,
                        Description = "Reklam ve Promosyon İçerikleri",
                        IsSystemProtected = true
                    },
                    new MessageCategory
                    {
                        Name = "Sistem / Güvenlik",
                        Icon = "shield",
                        Theme = CategoryTheme.RedTheme,
                        Description = "Sistem ve Güvenlik Uyarıları",
                        IsSystemProtected = true
                    },
                    new MessageCategory
                    {
                        Name = "Resmi / İş",
                        Icon = "business_center",
                        Theme = CategoryTheme.WhiteTheme,
                        Description = "Resmi Kurum ve İş Yazışmaları",
                        IsSystemProtected = true
                    },
                };

                await context.MessageCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }



        }



    }
}
