using IdentityMail.Web.Context;
using IdentityMail.Web.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.ViewComponents.Admin
{
    public class AdminMessageDetailViewComponent(AppDbContext _context) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int? id)
        {
            if (id == null)
            {
                return View("Empty");
            }

            var message = await _context.UserMessages.Include(x => x.Sender)
                                               .Include(x => x.Receiver)
                                               .Include(x => x.MessageCategory)
                                               .Include(x => x.MessageAttachments)
                                               .Where(x => x.Id == id)
                                               .FirstOrDefaultAsync();

            return View("Default", message);
        }
    }
}
