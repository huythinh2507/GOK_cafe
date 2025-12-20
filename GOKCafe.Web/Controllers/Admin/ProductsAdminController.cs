using GOKCafe.Application.DTOs.Product;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.Web.Controllers.Admin;

/// <summary>
/// Admin controller for managing products
/// </summary>
public class ProductsAdminController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IProductTypeService _productTypeService;
    private readonly ILogger<ProductsAdminController> _logger;

    public ProductsAdminController(
        IProductService productService,
        ICategoryService categoryService,
        IProductTypeService productTypeService,
        ILogger<ProductsAdminController> logger)
    {
        _productService = productService;
        _categoryService = categoryService;
        _productTypeService = productTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Display product list page
    /// </summary>
    [HttpGet]
    [Route("/admin/products")]
    public async Task<IActionResult> Index(
        int pageNumber = 1,
        int pageSize = 20,
        string? search = null,
        Guid? categoryId = null,
        bool? inStock = null)
    {
        try
        {
            var categoriesResult = await _categoryService.GetAllCategoriesAsync();
            var productTypesResult = await _productTypeService.GetAllProductTypesAsync();

            ViewBag.Categories = categoriesResult.Data;
            ViewBag.ProductTypes = productTypesResult.Data;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentInStock = inStock;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products admin page");
            return View("Error");
        }
    }

    /// <summary>
    /// Display create product page
    /// </summary>
    [HttpGet]
    [Route("/admin/products/create")]
    public async Task<IActionResult> Create()
    {
        try
        {
            var categoriesResult = await _categoryService.GetAllCategoriesAsync();
            var productTypesResult = await _productTypeService.GetAllProductTypesAsync();

            ViewBag.Categories = categoriesResult.Data;
            ViewBag.ProductTypes = productTypesResult.Data;

            return View("~/Views/ProductsAdmin/CreateProduct/Create.cshtml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create product page");
            return View("Error");
        }
    }

    /// <summary>
    /// Display edit product page
    /// </summary>
    [HttpGet]
    [Route("/admin/products/edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var productResult = await _productService.GetProductByIdAsync(id);
            if (!productResult.Success || productResult.Data == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction(nameof(Index));
            }

            var categoriesResult = await _categoryService.GetAllCategoriesAsync();
            var productTypesResult = await _productTypeService.GetAllProductTypesAsync();

            ViewBag.Categories = categoriesResult.Data;
            ViewBag.ProductTypes = productTypesResult.Data;
            ViewBag.SelectedProductTypeId = productResult.Data.ProductTypeId;
            ViewBag.SelectedCategoryId = productResult.Data.CategoryId;

            return View("~/Views/ProductsAdmin/EditProduct/Edit.cshtml", productResult.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit product page for ID: {ProductId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Display product details page
    /// </summary>
    [HttpGet]
    [Route("/admin/products/details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var productResult = await _productService.GetProductByIdAsync(id);
            if (!productResult.Success || productResult.Data == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/ProductsAdmin/DetailsProduct/Details.cshtml", productResult.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product details for ID: {ProductId}", id);
            return View("Error");
        }
    }
}
