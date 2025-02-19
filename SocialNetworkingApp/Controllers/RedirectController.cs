using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Data;

namespace SocialNetworkingApp.Controllers
{
    public class RedirectController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole(UserRoles.Admin)) return RedirectToAction("Index", "Admin");
            else return RedirectToAction("Index", "Feed");
        }
    }
}
