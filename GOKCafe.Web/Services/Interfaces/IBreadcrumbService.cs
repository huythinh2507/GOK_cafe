using GOKCafe.Web.Models.ViewModels;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace GOKCafe.Web.Services.Interfaces
{
    public interface IBreadcrumbService
    {
        /// <summary>
        /// Build breadcrumbs for product detail page
        /// </summary>
        List<BreadcrumbItem> BuildProductDetailBreadcrumbs(
            IPublishedContent? currentPage,
            string productName,
            string? categoryName = null);

        /// <summary>
        /// Build breadcrumbs from Umbraco content tree
        /// </summary>
        List<BreadcrumbItem> BuildUmbracoContentBreadcrumbs(IPublishedContent currentPage);

        /// <summary>
        /// Find Products page in content tree
        /// </summary>
        IPublishedContent? FindProductsPage(IPublishedContent? homeNode);
    }
}
