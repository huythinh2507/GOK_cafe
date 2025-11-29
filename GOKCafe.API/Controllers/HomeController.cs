using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.HomePage;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages homepage data operations in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Home API")]
public class HomeController : ControllerBase
{
    private readonly IHomeService _homeService;

    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    /// <summary>
    /// Get homepage data including featured categories and products
    /// </summary>
    /// <returns>Complete homepage data with banners, featured categories, and products</returns>
    [HttpGet]
    [ProducesResponseType<ApiResponse<HomePageDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<HomePageDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHomePageData()
    {
        var result = await _homeService.GetHomePageDataAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
