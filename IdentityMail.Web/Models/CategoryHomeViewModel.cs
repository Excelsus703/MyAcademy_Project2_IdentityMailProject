namespace IdentityMail.Web.Models
{
    public class CategoryHomeViewModel
    {
        public List<CategoryListViewModel> Categories { get; set; } = new();
        public int SelectedCategoryId { get; set; }
        public string SelectedCategoryName { get; set; } = string.Empty;
    }
}
