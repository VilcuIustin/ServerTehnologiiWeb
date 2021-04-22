using Microsoft.AspNetCore.Mvc;

namespace Server_Tehnologii_Web.Controllers
{
    public class MessageController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}