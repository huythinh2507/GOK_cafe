using GOKCafe.Web.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace GOKCafe.Web.Controllers
{
    public class PartnersController : RenderController
    {
        private readonly ILogger<PartnersController> _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public PartnersController(
            ILogger<PartnersController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        [ResponseCache(Duration = 300)]
        public override IActionResult Index()
        {
            var currentPage = CurrentPage;
            if (currentPage == null)
                return NotFound();

            try
            {
                _logger.LogInformation("PartnersController: Fetching partner items...");

                // Get all partner items (children of Partners page)
                var partnerItems = currentPage.Children<IPublishedContent>()
                    .Where(x => x.ContentType.Alias == "partnersItems" && x.IsPublished())
                    .Select(partner => new PartnerDto
                    {
                        Id = partner.Key,
                        Title = partner.Value<string>("title") ?? partner.Name,
                        ImageUrl = partner.Value<IPublishedContent>("image")?.Url() ?? ""
                    })
                    .ToList();

                _logger.LogInformation($"PartnersController: Found {partnerItems.Count} partner items");

                // Pass data to view
                ViewData["Title"] = currentPage.Value<string>("title") ?? currentPage.Name;
                ViewData["Description"] = currentPage.Value<string>("description") ?? "";
                ViewData["PartnerItems"] = partnerItems;

                return CurrentTemplate(CurrentPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PartnersController.Index");
                return NotFound();
            }
        }
    }
}
