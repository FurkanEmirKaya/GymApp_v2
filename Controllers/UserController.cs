using Microsoft.AspNetCore.Mvc;

namespace efCoreApp.AddControllers
{
    public class UserController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }
    }
}