using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.Web.Controllers.Admin;

public class DashboardController : Controller
{
    [HttpGet]
    [Route("/admin")]
    [Route("/admin/dashboard")]
    public IActionResult Index()
    {
        return View();
    }
}
