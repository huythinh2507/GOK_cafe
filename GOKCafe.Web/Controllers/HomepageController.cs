using GOKCafe.Web.Models.ViewModels;
using GOKCafe.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace GOKCafe.Web.Controllers
{
    // Controller for 'homepage' document type
    // Umbraco naming: [DocumentTypeAlias]Controller
    public class HomepageController : RenderController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public HomepageController(
            ILogger<HomepageController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IProductService productService,
            ICategoryService categoryService)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _productService = productService;
            _categoryService = categoryService;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public override IActionResult Index()
        {
            var homepage = CurrentPage;
            if (homepage == null)
            {
                return NotFound();
            }

            // Get dynamic data
            var featuredCount = homepage.Value<int>("featuredProductsCount");
            if (featuredCount == 0) featuredCount = 8;

            var featuredProducts = _productService.GetFeaturedProducts(featuredCount).ToList();
            var categories = _categoryService.GetAllCategories().ToList();

            // Pass data to view via ViewData so view can access it
            ViewData["FeaturedProducts"] = featuredProducts;
            ViewData["Categories"] = categories;

            // Return CurrentTemplate with CurrentPage for Umbraco routing
            return CurrentTemplate(CurrentPage);
        }

        private SiteSettings GetSiteSettings()
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
                return new SiteSettings();

            var settingsNode = umbracoContext.Content?
                .GetAtRoot()
                .FirstOrDefault(x => x.ContentType.Alias == "settings");

            if (settingsNode == null)
                return new SiteSettings();

            return new SiteSettings
            {
                SiteName = settingsNode.Value<string>("siteName") ?? "GOK Cafe",
                LogoUrl = settingsNode.Value<IPublishedContent>("logo")?.Url() ?? string.Empty,
                Phone = settingsNode.Value<string>("phone") ?? string.Empty,
                Email = settingsNode.Value<string>("email") ?? string.Empty,
                Address = settingsNode.Value<string>("address") ?? string.Empty,
                FacebookUrl = settingsNode.Value<string>("facebookUrl") ?? string.Empty,
                InstagramUrl = settingsNode.Value<string>("instagramUrl") ?? string.Empty,
                TwitterUrl = settingsNode.Value<string>("twitterUrl") ?? string.Empty,
                CopyrightText = settingsNode.Value<string>("copyrightText") ?? $"© {DateTime.Now.Year} GOK Cafe. All rights reserved."
            };
        }
    }
}
