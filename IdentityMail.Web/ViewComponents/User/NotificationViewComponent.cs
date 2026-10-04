using IdentityMail.Web.Context;
using IdentityMail.Web.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.ViewComponents.User
{
    public class NotificationViewComponent(UserManager<AppUser> _userManager,
                                    AppDbContext _context) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return Content(string.Empty);
            }

            var baseQuery = _context.UserMessages.Where(x => x.ReceiverId == user.Id && x.isRead == false && x.isDraft == false && x.isTrash == false);

            ViewBag.TotalUnreadCount = await baseQuery.CountAsync();

            var unreadMessages = await baseQuery.Include(x => x.Sender)
                                                .OrderByDescending(x => x.SendDate)
                                                .Take(3)
                                                .ToListAsync();

            return View(unreadMessages);
        }
    }
}
