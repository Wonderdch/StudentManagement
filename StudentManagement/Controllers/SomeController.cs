using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudentManagement.Controllers
{
    [Authorize(Roles = "Admin, User")]
    public class SomeController : Controller
    {
        public string ABC()
        {
            return "ABC操作方法";
        }

        [Authorize(Roles = "Admin")]
        public string XYZ()
        {
            return "XYZ操作方法";
        }

        [AllowAnonymous]
        public string Anyone()
        {
            return "Anyone操作方法";
        }
    }
}