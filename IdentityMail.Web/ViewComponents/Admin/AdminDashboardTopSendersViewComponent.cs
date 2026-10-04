using IdentityMail.Web.Context;
using IdentityMail.Web.Entites;
using IdentityMail.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.ViewComponents.Admin
{
    public class AdminDashboardTopSendersViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminDashboardTopSendersViewComponent(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var topSendersData = await _context.UserMessages.GroupBy(m => new { m.SenderId, m.Sender.UserName })
                                                        .Select(g => new
                                                        {
                                                            SenderId = g.Key.SenderId,
                                                            UserName = g.Key.UserName,
                                                            MessageCount = g.Count()
                                                        })
                                                        .OrderByDescending(x => x.MessageCount).Take(7).ToListAsync();

            var resultList = new List<AdminDashboardTopSenderViewModel>();

            foreach (var item in topSendersData)
            {
                var user = await _userManager.FindByIdAsync(item.SenderId.ToString());
                var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

                resultList.Add(new AdminDashboardTopSenderViewModel
                {
                    UserName = item.UserName,
                    MessageCount = item.MessageCount,
                    Role = roles.FirstOrDefault()
                });
            }

            ViewBag.TotalUsers = await _context.Users.CountAsync();

            return View(resultList);
        }
    }
}
