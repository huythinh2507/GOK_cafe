using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [HttpGet]
    public async Task<IActionResult> GetHomePageData()
    {
        var result = await _homeService.GetHomePageDataAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
