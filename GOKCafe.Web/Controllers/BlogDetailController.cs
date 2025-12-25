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

        [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "id" })]
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

                // Get content - handle both string and BlockGrid
                string content = "";
                var contentValue = blogNode.Value("content");

                if (contentValue is string strContent)
                {
                    content = strContent;
                }
                else if (contentValue is Umbraco.Cms.Core.Models.Blocks.BlockGridModel blockGrid)
                {
                    // Convert BlockGrid to HTML string
                    // Each block has a "content" property with the rich text HTML
                    var htmlParts = new List<string>();
                    foreach (var item in blockGrid)
                    {
                        // Try to get the "content" property from each Rich Text Block
                        var blockContent = item.Content.Value<string>("content");
                        if (!string.IsNullOrWhiteSpace(blockContent))
                        {
                            htmlParts.Add(blockContent);
                        }
                    }
                    content = string.Join("", htmlParts);
                }
                else if (contentValue != null)
                {
                    content = contentValue.ToString() ?? "";
                }

                // Get tags
                var tags = new List<string>();
                var tagProperty = blogNode.GetProperty("tagName");
                if (tagProperty != null)
                {
                    var tagValue = tagProperty.GetValue();
                    if (tagValue is IEnumerable<string> tagList)
                    {
                        tags = tagList.ToList();
                    }
                    else if (tagValue is string tagStr && !string.IsNullOrEmpty(tagStr))
                    {
                        tags = tagStr.Split(',').Select(t => t.Trim()).ToList();
                    }
                }

                // Map to BlogDto
                var blog = new BlogDto
                {
                    Id = blogNode.Key,
                    Title = blogNode.Value<string>("title") ?? blogNode.Name,
                    Slug = blogNode.UrlSegment,
                    Excerpt = blogNode.Value<string>("excerpt") ?? "",
                    Content = content,
                    FeaturedImageUrl = blogNode.Value<IPublishedContent>("image")?.Url() ?? "",
                    PublishedDate = blogNode.Value<DateTime>("publishDate") != default
                        ? blogNode.Value<DateTime>("publishDate")
                        : DateTime.Now,
                    Author = blogNode.Value<string>("author") ?? "GOK Cafe",
                    IsPublished = true,
                    Tags = tags
                };

                _logger.LogInformation($"BlogDetailController: Successfully loaded blog: {blog.Title}");
                _logger.LogInformation($"  - Content length: {blog.Content?.Length ?? 0}");
                _logger.LogInformation($"  - Tags: {string.Join(", ", blog.Tags)}");

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
