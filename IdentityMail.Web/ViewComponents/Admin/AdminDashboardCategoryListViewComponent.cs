using IdentityMail.Web.Context;
using IdentityMail.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.ViewComponents.Admin
{
    public class AdminDashboardCategoryListViewComponent(AppDbContext _context) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {

            var categories = await _context.MessageCategories
                                            .Select(c => new AdminCategoryListViewModel
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Description = c.Description,
                                                ColorTheme = (int)c.Theme,
                                                MessageCount = _context.UserMessages.Count(m => m.MessageCategoryId == c.Id)
                                            }).Take(5).ToListAsync();

            return View(categories);
        }
    }
}
