using IdentityMail.Web.Context;
using IdentityMail.Web.Entites;
using IdentityMail.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.ViewComponents.User
{
    public class MessageSearchFilterViewComponent(UserManager<AppUser> _userManager,
                                    AppDbContext _context) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(MessageSearchFilterViewModel messageSearchFilterViewModel, int? selectedId)
        {
            messageSearchFilterViewModel ??= new MessageSearchFilterViewModel();

            var user = await _userManager.GetUserAsync(HttpContext.User);

            var query = _context.UserMessages.Include(x => x.Sender)
                                             .Include(x => x.MessageCategory)
                                             .Where(x => x.ReceiverId == user.Id && x.isTrash == false && x.isDraft == false)
                                             .AsQueryable();

            // İsim veya Konu Filtresi
            if (!string.IsNullOrWhiteSpace(messageSearchFilterViewModel.SearchTerm))
            {
                var term = messageSearchFilterViewModel.SearchTerm.ToLower().Trim();
                query = query.Where(m =>
                m.Subject.ToLower().Contains(term) ||
                m.Sender.FirstName.ToLower().Contains(term) ||
                m.Sender.LastName.ToLower().Contains(term)
                );
            }

            // Kategori Filtresi
            if (messageSearchFilterViewModel.CategoryId.HasValue)
            {
                query = query.Where(m => m.MessageCategoryId == messageSearchFilterViewModel.CategoryId.Value);
            }

            // Başlangıç Tarihi
            if (messageSearchFilterViewModel.StartDate.HasValue)
            {
                query = query.Where(m => m.SendDate >= messageSearchFilterViewModel.StartDate.Value);
            }

            // Bitiş Tarihi (Gün sonuna kadar)
            if (messageSearchFilterViewModel.EndDate.HasValue)
            {
                var endDate = messageSearchFilterViewModel.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(m => m.SendDate <= endDate);
            }

            ViewBag.SelectedId = selectedId;
            var filteredMessages = await query.OrderByDescending(m => m.SendDate).ToListAsync();

            return View(filteredMessages);
        }
    }
}
