using GOKCafe.Web.Models.ViewModels;
using GOKCafe.Web.Services.Interfaces;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace GOKCafe.Web.Services.Implementations
{
    public class BreadcrumbService : IBreadcrumbService
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public BreadcrumbService(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Build breadcrumbs for product detail page
        /// </summary>
        public List<BreadcrumbItem> BuildProductDetailBreadcrumbs(
            IPublishedContent? currentPage,
            string productName,
            string? categoryName = null)
        {
            var breadcrumbs = new List<BreadcrumbItem>();

            // Get home node
            var homeNode = GetHomeNode();
            if (homeNode != null)
            {
                breadcrumbs.Add(new BreadcrumbItem
                {
                    Name = homeNode.Name,
                    Url = homeNode.Url(),
                    IsActive = false
                });
            }

            // Add Products page - Always add it
            var productsNode = FindProductsPage(homeNode);
            if (productsNode != null)
            {
                breadcrumbs.Add(new BreadcrumbItem
                {
                    Name = productsNode.Name,
                    Url = productsNode.Url(),
                    IsActive = false
                });
            }
            else
            {
                // Fallback: If Products page not found in content tree, add it manually
                // This ensures breadcrumb always shows: Home > Products > Product
                breadcrumbs.Add(new BreadcrumbItem
                {
                    Name = "Products",
                    Url = "/products",
                    IsActive = false
                });
            }

            // Optionally add category as intermediate level
            if (!string.IsNullOrEmpty(categoryName))
            {
                // Only add if you have category landing pages
                // breadcrumbs.Add(new BreadcrumbItem
                // {
                //     Name = categoryName,
                //     Url = $"/products?category={categoryName}",
                //     IsActive = false
                // });
            }

            // Add current product
            breadcrumbs.Add(new BreadcrumbItem
            {
                Name = productName,
                Url = string.Empty,
                IsActive = true
            });

            return breadcrumbs;
        }

        /// <summary>
        /// Build breadcrumbs from Umbraco content tree
        /// </summary>
        public List<BreadcrumbItem> BuildUmbracoContentBreadcrumbs(IPublishedContent currentPage)
        {
            var breadcrumbs = new List<BreadcrumbItem>();

            var homeNode = GetHomeNode();
            if (homeNode != null)
            {
                breadcrumbs.Add(new BreadcrumbItem
                {
                    Name = homeNode.Name,
                    Url = homeNode.Url(),
                    IsActive = false
                });

                // Add ancestors
                var ancestors = currentPage.Ancestors().Where(x => x.Level > 1).Reverse();
                foreach (var ancestor in ancestors)
                {
                    breadcrumbs.Add(new BreadcrumbItem
                    {
                        Name = ancestor.Name,
                        Url = ancestor.Url(),
                        IsActive = false
                    });
                }
            }

            // Add current page
            breadcrumbs.Add(new BreadcrumbItem
            {
                Name = currentPage.Name,
                Url = currentPage.Url(),
                IsActive = true
            });

            return breadcrumbs;
        }

        /// <summary>
        /// Find the Products page in the content tree using multiple strategies
        /// </summary>
        public IPublishedContent? FindProductsPage(IPublishedContent? homeNode)
        {
            if (homeNode == null)
                return null;

            // Strategy 1: Find by content type alias (most reliable)
            var productsNode = homeNode.Children()?
                .FirstOrDefault(x => x.ContentType.Alias == "products" ||
                                   x.ContentType.Alias == "productList" ||
                                   x.ContentType.Alias == "productPage");

            if (productsNode != null)
                return productsNode;

            // Strategy 2: Find by name (case-insensitive)
            productsNode = homeNode.Children()?
                .FirstOrDefault(x => x.Name.Equals("Products", StringComparison.OrdinalIgnoreCase) ||
                                   x.Name.Equals("Product", StringComparison.OrdinalIgnoreCase));

            if (productsNode != null)
                return productsNode;

            // Strategy 3: Find by URL segment
            productsNode = homeNode.Children()?
                .FirstOrDefault(x => x.UrlSegment?.Equals("products", StringComparison.OrdinalIgnoreCase) == true);

            return productsNode;
        }

        /// <summary>
        /// Get home node from content tree
        /// </summary>
        private IPublishedContent? GetHomeNode()
        {
            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            return umbracoContext.Content?.GetAtRoot()
                .FirstOrDefault(x => x.ContentType.Alias == "home" || x.ContentType.Alias == "homepage");
        }
    }
}
