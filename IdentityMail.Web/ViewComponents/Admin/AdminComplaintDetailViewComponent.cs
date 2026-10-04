using IdentityMail.Web.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.ViewComponents.Admin
{
    public class AdminComplaintDetailViewComponent(AppDbContext _context) : ViewComponent
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
                                                     .FirstOrDefaultAsync(x => x.Id == id);

            if (message == null)
            {
                return View("Empty");
            }

            return View("Default", message);
        }
    }
}
