using Microsoft.AspNetCore.Mvc;

namespace CURDExample.Controllers
{
    [Route("Test")]
    public class TestController : Controller
    {
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
