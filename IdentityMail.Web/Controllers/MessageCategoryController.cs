using IdentityMail.Web.Context;
using IdentityMail.Web.Entites;
using IdentityMail.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.Controllers
{
    [Authorize]
    public class MessageCategoryController(UserManager<AppUser> _userManager,
                                    AppDbContext _context) : Controller
    {
        public async Task<IActionResult> CategoryHome(int? categoryId)
        {
            var user = await _userManager.GetUserAsync(User);

            var categories = await _context.MessageCategories
                                            .Select(category => new CategoryListViewModel
                                            {
                                                Id = category.Id,
                                                Name = category.Name,
                                                Icon = category.Icon,
                                                Theme = category.Theme,
                                                UnreadCount = _context.UserMessages.Count(message =>
                                                                                    message.MessageCategoryId == category.Id &&
                                                                                    message.ReceiverId == user.Id &&
                                                                                    message.isRead == false &&
                                                                                    message.isDraft == false)
                                            }).ToListAsync();

            var selectedCategory = categories.FirstOrDefault(c => c.Id == categoryId)
                           ?? categories.FirstOrDefault();

            var model = new CategoryHomeViewModel
            {
                Categories = categories,
                SelectedCategoryId = selectedCategory?.Id ?? 1,
                SelectedCategoryName = selectedCategory?.Name ?? "Kategori Yok"
            };

            ViewBag.ActiveTab = "CategoryHome";

            return View(model);
        }
    }
}
