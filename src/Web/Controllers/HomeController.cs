using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();  
        public IActionResult Privacy() => View();
    }
}
