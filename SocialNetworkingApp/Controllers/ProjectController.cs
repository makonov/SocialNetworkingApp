using Microsoft.AspNetCore.Mvc;

namespace SocialNetworkingApp.Controllers
{
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details()
        {
            return View();
        }
    }
}
