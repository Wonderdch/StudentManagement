using Microsoft.AspNetCore.Mvc;

namespace StudentManagement.Controllers
{
    public class HomeController : Controller
    {
        // GET: /<controller>/
        public string Index()
        {
            return "Hello World from MVC";
        }
    }
}
