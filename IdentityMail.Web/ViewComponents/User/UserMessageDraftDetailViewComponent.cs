using IdentityMail.Web.Context;
using IdentityMail.Web.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.ViewComponents.User
{
    public class UserMessageDraftDetailViewComponent(UserManager<AppUser> _userManager,
                                                AppDbContext _context) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int? id)
        {
            if (id == null)
            {
                return View("Empty");
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);

            var message = await _context.UserMessages.Include(x => x.Receiver)
                                                     .Include(x => x.MessageAttachments)
                                                     .FirstOrDefaultAsync(x => x.Id == id && x.SenderId == user.Id && x.isDraft == true);

            ViewBag.Categories = new SelectList(
                await _context.MessageCategories.ToListAsync(),
                "Id",
                "Name",
                message?.MessageCategoryId
            );

            if (message == null)
            {
                return View("Empty");
            }

            return View("Default", message);
        }
    }
}
