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

        [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "page", "search" })]
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

                // LOG: Debug Umbraco data
                _logger.LogInformation("=== CHECKING UMBRACO BLOG DATA ===");
                foreach (var blog in allBlogs.Take(1)) // Only log first blog to avoid spam
                {
                    _logger.LogInformation($"Blog Name: {blog.Name}");
                    _logger.LogInformation($"Blog ID: {blog.Key}");
                    _logger.LogInformation($"Content Type: {blog.ContentType.Alias}");

                    // List all available properties with values
                    _logger.LogInformation("All Properties:");
                    foreach (var prop in blog.Properties)
                    {
                        var value = prop.GetValue();
                        var valueType = value?.GetType().Name ?? "null";
                        var valuePreview = value != null ?
                            (value.ToString()?.Length > 50 ? value.ToString()?.Substring(0, 50) + "..." : value.ToString())
                            : "null";
                        _logger.LogInformation($"  - {prop.Alias} ({prop.PropertyType.EditorAlias}) = [{valueType}] {valuePreview}");
                    }

                    // Try different ways to get tagName
                    _logger.LogInformation("\n>>> Trying to get TagName:");
                    _logger.LogInformation($"  HasValue('tagName'): {blog.HasValue("tagName")}");

                    var tagNameProp = blog.GetProperty("tagName");
                    if (tagNameProp != null)
                    {
                        var tagValue = tagNameProp.GetValue();
                        var tagValueType = tagValue?.GetType();
                        _logger.LogInformation($"  Raw type: {tagValueType?.FullName}");
                        _logger.LogInformation($"  Is IEnumerable<string>: {tagValue is IEnumerable<string>}");
                        _logger.LogInformation($"  Is string: {tagValue is string}");
                        _logger.LogInformation($"  Is array: {tagValue is Array}");

                        if (tagValue != null)
                        {
                            var json = System.Text.Json.JsonSerializer.Serialize(tagValue);
                            _logger.LogInformation($"  JSON value: {json}");
                            _logger.LogInformation($"  ToString(): {tagValue}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation("  tagName property is NULL");
                    }

                    // Try different ways to get content
                    _logger.LogInformation("\n>>> Trying to get Content:");
                    var contentProp = blog.GetProperty("content");
                    if (contentProp != null)
                    {
                        var contentValue = contentProp.GetValue();
                        var contentType = contentValue?.GetType();
                        _logger.LogInformation($"  Raw type: {contentType?.FullName}");
                        _logger.LogInformation($"  Is string: {contentValue is string}");
                        _logger.LogInformation($"  Is BlockGridModel: {contentValue is Umbraco.Cms.Core.Models.Blocks.BlockGridModel}");

                        if (contentValue is Umbraco.Cms.Core.Models.Blocks.BlockGridModel blockGrid)
                        {
                            _logger.LogInformation($"  BlockGrid count: {blockGrid.Count()}");
                            foreach (var item in blockGrid.Take(1))
                            {
                                _logger.LogInformation($"  Block content type: {item.Content.ContentType.Alias}");
                                _logger.LogInformation($"  Block properties:");
                                foreach (var prop in item.Content.Properties)
                                {
                                    var val = prop.GetValue();
                                    _logger.LogInformation($"    - {prop.Alias}: {val?.ToString()?.Substring(0, Math.Min(100, val?.ToString()?.Length ?? 0))}");
                                }
                            }
                        }
                        else if (contentValue is string strContent)
                        {
                            _logger.LogInformation($"  String content (first 200 chars): {strContent.Substring(0, Math.Min(200, strContent.Length))}");
                        }
                    }
                }
                _logger.LogInformation("====================================");

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
                    .Select(blog => {
                        // Get content - handle both string and BlockGrid
                        string content = "";
                        var contentValue = blog.Value("content");

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
                            // Fallback: try to convert to string
                            content = contentValue.ToString() ?? "";
                        }

                        // Get tags - handle different property types
                        var tags = new List<string>();
                        var tagProperty = blog.GetProperty("tagName");
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

                        return new BlogDto
                        {
                            Id = blog.Key,
                            Title = blog.Value<string>("title") ?? blog.Name,
                            Slug = blog.UrlSegment,
                            Excerpt = blog.Value<string>("excerpt") ?? "",
                            Content = content,
                            FeaturedImageUrl = blog.Value<IPublishedContent>("image")?.Url() ?? "",
                            PublishedDate = blog.Value<DateTime>("publishDate") != default
                                ? blog.Value<DateTime>("publishDate")
                                : DateTime.Now,
                            Author = blog.Value<string>("author") ?? "GOK Cafe",
                            IsPublished = true,
                            Tags = tags
                        };
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
