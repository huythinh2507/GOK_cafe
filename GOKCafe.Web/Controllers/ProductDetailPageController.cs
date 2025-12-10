using Microsoft.AspNetCore.Mvc;
using GOKCafe.Web.Services.Interfaces;

namespace GOKCafe.Web.Controllers
{
    public class ProductDetailPageController : Controller
    {
        private readonly IProductService _productService;

        public ProductDetailPageController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            ViewData["Product"] = product;
            return View("~/Views/productDetail.cshtml", product);
        }

        // Alternative route that accepts slug instead of ID
        [Route("product/{slug}")]
        public async Task<IActionResult> BySlug(string slug)
        {
            var product = await _productService.GetProductBySlugAsync(slug);

            if (product == null)
                return NotFound();

            ViewData["Product"] = product;
            return View("~/Views/productDetail.cshtml", product);
        }
    }
}
