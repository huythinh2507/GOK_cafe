using GOKCafe.Web.Models.DTOs;

namespace GOKCafe.Web.Models.ViewModels
{
    public class BlogDetailViewModel
    {
        public BlogDto Blog { get; set; } = new();
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
    }
}
