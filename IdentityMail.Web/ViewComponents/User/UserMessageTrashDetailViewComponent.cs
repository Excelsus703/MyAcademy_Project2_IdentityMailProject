using IdentityMail.Web.Context;
using IdentityMail.Web.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.ViewComponents.User
{
    public class UserMessageTrashDetailViewComponent(UserManager<AppUser> _userManager,
                                                AppDbContext _context) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int? id)
        {
            if (id == null)
            {
                return View("Empty");
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);

            var message = await _context.UserMessages.Include(x => x.Sender)
                                                     .Include(x => x.MessageAttachments)
                                                     .FirstOrDefaultAsync(x => x.Id == id && x.ReceiverId == user.Id);

            if (message == null)
            {
                return View("Empty");
            }

            return View("Default", message);
        }
    }
}
