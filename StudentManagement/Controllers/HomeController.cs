using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using StudentManagement.ViewModels;

namespace StudentManagement.Controllers
{
    public class HomeController : Controller
    {
        // 通过 readonly 保证我们只能在构造器中初始化它，不会在其它地方误修改
        private readonly IStudentRepository _studentRepository;

        private readonly IHostingEnvironment _hostingEnvironment;

        // 使用构造函数注入的方式注入 IStudentRepository
        public HomeController(IStudentRepository studentRepository, IHostingEnvironment hostingEnvironment)
        {
            _studentRepository = studentRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        public ViewResult Index()
        {
            // 查询所有的学生信息
            var students = _studentRepository.GetAllStudents();
            // 将学生列表传递到视图
            return View(students);
        }

        public IActionResult Details(int? id)
        {
            // 实例化 HomeDetailsViewModel 并存储 Student 详细信息和 PageTitle
            var homeDetailsViewModel = new HomeDetailsViewModel
            {
                Student = _studentRepository.GetStudent(id ?? 1),
                PageTitle = "学生详细信息"
            };

            // 将ViewModel对象传递给View()方法
            return View(homeDetailsViewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(StudentCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;

                if (model.Photo != null)
                {
                    var uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid() + "_" + model.Photo.FileName;

                    var filePath = Path.Combine(uploadFolder, uniqueFileName);

                    model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                }

                var student = new Student
                {
                    Name = model.Name,
                    Email = model.Email,
                    ClassName = model.ClassName,
                    PhotoPath = uniqueFileName
                };

                var newStudent = _studentRepository.Add(student);

                return RedirectToAction("Details", new { id = newStudent.Id });
            }

            return View();
        }

        [HttpGet]
        public ViewResult Edit(int id)
        {
            var student = _studentRepository.GetStudent(id);

            var stuentEditViewModel = new StudentEditViewModel
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                ClassName = student.ClassName,
                ExistingPhotoPath = student.PhotoPath
            };

            return View(stuentEditViewModel);
        }
    }
}
