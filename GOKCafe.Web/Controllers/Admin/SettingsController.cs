using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.Web.Controllers.Admin;

public class SettingsController : Controller
{
    [HttpGet]
    [Route("/admin/settings")]
    public IActionResult CustomField()
    {
        return View("~/Views/Admin/Settings/CustomField/CustomField.cshtml");
    }
}
