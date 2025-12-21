using Microsoft.AspNetCore.Mvc;

namespace OnlineSalesManagementSystem.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}