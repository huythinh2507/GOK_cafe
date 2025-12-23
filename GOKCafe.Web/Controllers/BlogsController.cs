using GOKCafe.Web.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace GOKCafe.Web.Controllers
{
    public class BlogsController : RenderController
    {
        private readonly ILogger<BlogsController> _logger;

        public BlogsController(
            ILogger<BlogsController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _logger = logger;
        }

        public override IActionResult Index()
        {
            var currentPage = CurrentPage;
            if (currentPage == null)
                return NotFound();

            try
            {
                // Get query parameters
                var pageNumber = int.TryParse(Request.Query["page"], out var page) ? page : 1;
                var searchTerm = Request.Query["search"].ToString();
                var pageSize = 9; // 3x3 grid

                _logger.LogInformation($"BlogsController: Fetching blogs - Page: {pageNumber}, PageSize: {pageSize}, Search: {searchTerm}");

                // Get all blog items from Umbraco (children of current Blog page)
                var allBlogs = currentPage.Children?
                    .Where(x => x.ContentType.Alias == "blogsItem")
                    .OrderByDescending(x => x.Value<DateTime>("publishDate"))
                    .ToList() ?? new List<IPublishedContent>();

                // Apply search filter (case-insensitive field names)
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    allBlogs = allBlogs.Where(blog =>
                        blog.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (blog.Value<string>("title")?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (blog.Value<string>("content")?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }

                // Calculate pagination
                var totalItems = allBlogs.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages > 0 ? totalPages : 1));

                // Get paginated blogs
                var paginatedBlogs = allBlogs
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(blog => new BlogDto
                    {
                        Id = blog.Key,
                        Title = blog.Value<string>("title") ?? blog.Name,
                        Slug = blog.UrlSegment,
                        Excerpt = blog.Value<string>("excerpt") ?? "",
                        Content = blog.Value<string>("content") ?? "",
                        FeaturedImageUrl = blog.Value<IPublishedContent>("image")?.Url() ?? "",
                        PublishedDate = blog.Value<DateTime>("publishDate") != default
                            ? blog.Value<DateTime>("publishDate")
                            : DateTime.Now,
                        Author = blog.Value<string>("author") ?? "GOK Cafe",
                        IsPublished = true
                    })
                    .ToList();

                _logger.LogInformation($"BlogsController: Got {paginatedBlogs.Count} blogs from Umbraco");

                // Create paginated response
                var paginatedResponse = new PaginatedResponse<BlogDto>
                {
                    Items = paginatedBlogs,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                // Pass data to view via ViewData
                ViewData["Blogs"] = paginatedResponse;
                ViewData["SearchTerm"] = searchTerm;

                // Return CurrentTemplate with CurrentPage
                return CurrentTemplate(CurrentPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BlogsController.Index");

                // Return empty data so page doesn't crash
                ViewData["Blogs"] = new PaginatedResponse<BlogDto>
                {
                    Items = new List<BlogDto>(),
                    PageNumber = 1,
                    PageSize = 9,
                    TotalItems = 0,
                    TotalPages = 0
                };
                ViewData["SearchTerm"] = "";

                return CurrentTemplate(CurrentPage);
            }
        }
    }
}
