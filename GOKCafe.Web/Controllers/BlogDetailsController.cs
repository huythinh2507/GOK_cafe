using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace GOKCafe.Web.Controllers
{
    public class BlogDetailsController : RenderController
    {
        private readonly ILogger<BlogDetailsController> _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public BlogDetailsController(
            ILogger<BlogDetailsController> logger,
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

                _logger.LogInformation($"BlogDetailsController: Fetching blog with ID: {blogIdString}");

                if (string.IsNullOrEmpty(blogIdString) || !Guid.TryParse(blogIdString, out var blogId))
                {
                    _logger.LogWarning("BlogDetailController: Invalid or missing blog ID");
                    return NotFound();
                }

                // Get blog from Umbraco by ID
                var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
                var blogNode = umbracoContext.Content?.GetById(blogId);

                _logger.LogInformation($"BlogDetailController: Blog node retrieved: {blogNode != null}");
                if (blogNode != null)
                {
                    _logger.LogInformation($"BlogDetailController: Blog ContentType Alias: {blogNode.ContentType.Alias}");
                    _logger.LogInformation($"BlogDetailController: Blog Name: {blogNode.Name}");
                    _logger.LogInformation($"BlogDetailController: Blog IsPublished: {blogNode.IsPublished()}");
                }

                if (blogNode == null || blogNode.ContentType.Alias != "blogsItem")
                {
                    _logger.LogWarning($"BlogDetailController: Blog not found or wrong content type. BlogNode null: {blogNode == null}, ContentType: {blogNode?.ContentType?.Alias ?? "N/A"}");
                    ViewData["Blog"] = null;
                    ViewData["Error"] = $"Blog not found. ID: {blogId}";
                    return CurrentTemplate(CurrentPage);
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

                // Create breadcrumbs
                var breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Name = "Home", Url = "/", IsActive = false },
                    new BreadcrumbItem { Name = "Blog", Url = "/blogs", IsActive = false },
                    new BreadcrumbItem { Name = blog.Title, Url = "", IsActive = true }
                };

                // Create ViewModel
                var viewModel = new BlogDetailViewModel
                {
                    Blog = blog,
                    Breadcrumbs = breadcrumbs
                };

                // Pass data to view
                ViewData["Blog"] = blog;
                ViewData["BlogDetailViewModel"] = viewModel;

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
