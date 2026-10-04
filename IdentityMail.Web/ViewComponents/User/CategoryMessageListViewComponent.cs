using IdentityMail.Web.Context;
using IdentityMail.Web.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.ViewComponents.User
{
    public class CategoryMessageListViewComponent(UserManager<AppUser> _userManager,
                                    AppDbContext _context) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int categoryId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var messages = await _context.UserMessages.Include(m => m.Sender)
                                                      .Where(m => m.MessageCategoryId == categoryId && m.ReceiverId == user.Id && m.isTrash == false && m.isDraft == false)
                                                      .OrderByDescending(m => m.SendDate)
                                                      .ToListAsync();

            return View(messages);
        }
    }
}
