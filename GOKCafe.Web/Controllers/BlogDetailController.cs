using GOKCafe.Web.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace GOKCafe.Web.Controllers
{
    public class BlogDetailController : RenderController
    {
        private readonly ILogger<BlogDetailController> _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public BlogDetailController(
            ILogger<BlogDetailController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public override IActionResult Index()
        {
            var currentPage = CurrentPage;
            if (currentPage == null)
                return NotFound();

            try
            {
                // Get blog ID from query string
                var blogIdString = Request.Query["id"].ToString();

                _logger.LogInformation($"BlogDetailController: Fetching blog with ID: {blogIdString}");

                if (string.IsNullOrEmpty(blogIdString) || !Guid.TryParse(blogIdString, out var blogId))
                {
                    _logger.LogWarning("BlogDetailController: Invalid or missing blog ID");
                    return NotFound();
                }

                // Get blog from Umbraco by ID
                var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
                var blogNode = umbracoContext.Content?.GetById(blogId);

                if (blogNode == null || blogNode.ContentType.Alias != "blogsItem")
                {
                    _logger.LogWarning($"BlogDetailController: Blog not found with ID: {blogId}");
                    return NotFound();
                }

                // Map to BlogDto
                var blog = new BlogDto
                {
                    Id = blogNode.Key,
                    Title = blogNode.Value<string>("title") ?? blogNode.Name,
                    Slug = blogNode.UrlSegment,
                    Excerpt = blogNode.Value<string>("excerpt") ?? "",
                    Content = blogNode.Value<string>("content") ?? "",
                    FeaturedImageUrl = blogNode.Value<IPublishedContent>("image")?.Url() ?? "",
                    PublishedDate = blogNode.Value<DateTime>("publishDate") != default
                        ? blogNode.Value<DateTime>("publishDate")
                        : DateTime.Now,
                    Author = blogNode.Value<string>("author") ?? "GOK Cafe",
                    IsPublished = true
                };

                _logger.LogInformation($"BlogDetailController: Successfully loaded blog: {blog.Title}");

                // Pass data to view
                ViewData["Blog"] = blog;

                return CurrentTemplate(CurrentPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BlogDetailController.Index");
                return NotFound();
            }
        }
    }
}
