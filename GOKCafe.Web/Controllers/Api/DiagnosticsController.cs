using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Web;

namespace GOKCafe.Web.Controllers.Api
{
    /// <summary>
    /// Provides diagnostic information for the Umbraco CMS content structure
    /// </summary>
    [Route("api/v1/diagnostics")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Diagnostics API")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(
            IUmbracoContextAccessor umbracoContextAccessor,
            ILogger<DiagnosticsController> logger)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Get the Umbraco content tree structure
        /// </summary>
        /// <returns>The content tree with root nodes and their children</returns>
        [HttpGet("content-tree")]
        [ProducesResponseType<object>(StatusCodes.Status200OK)]
        public IActionResult GetContentTree()
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return Ok(new { error = "Could not get Umbraco context" });
            }

            var rootNodes = umbracoContext.Content?.GetAtRoot();
            if (rootNodes == null)
            {
                return Ok(new { error = "No root nodes found" });
            }

            var result = rootNodes.Select(node => new
            {
                id = node.Id,
                key = node.Key,
                name = node.Name,
                documentTypeAlias = node.ContentType.Alias,
                url = node.Url(),
                culture = node.Cultures.Keys.ToList(),
                children = node.Children?.Select(child => new
                {
                    id = child.Id,
                    name = child.Name,
                    documentTypeAlias = child.ContentType.Alias,
                    url = child.Url()
                }).ToList()
            }).ToList();

            return Ok(new
            {
                success = true,
                rootNodesCount = rootNodes.Count(),
                nodes = result
            });
        }
    }
}
