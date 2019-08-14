using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using StudentManagement.ViewModels;

namespace StudentManagement.Controllers
{
    public class HomeController : Controller
    {
        // 通过 readonly 保证我们只能在构造器中初始化它，不会在其它地方误修改
        private readonly IStudentRepository _studentRepository;

        // 使用构造函数注入的方式注入 IStudentRepository
        public HomeController(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public ViewResult Index()
        {
            // 查询所有的学生信息
            var model = _studentRepository.GetAllStudents();
            // 将学生列表传递到视图
            return View(model);
        }

        public IActionResult Details()
        {
            // 实例化 HomeDetailsViewModel 并存储 Student 详细信息和 PageTitle
            var homeDetailsViewModel = new HomeDetailsViewModel
            {
                Student = _studentRepository.GetStudent(1),
                PageTitle = "学生详细信息"
            };

            // 将ViewModel对象传递给View()方法
            return View(homeDetailsViewModel);
        }
    }
}
