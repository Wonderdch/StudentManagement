﻿using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;

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

        public string Index()
        {
            return _studentRepository.GetStudent(1).Name;
        }

        public IActionResult Details()
        {
            var model = _studentRepository.GetStudent(1);

            // 绝对路径 需要带 .cshtml 文件扩展名
            //return View("MyViews/Test.cshtml"); 
            // 绝对路径（推荐写法）
            //return View("~/MyViews/Test.cshtml");

            // 相对路径 无需带扩展名
            // 相对路径的查找起点依然时 Home/Details
            return View("../../MyViews/Test");
        }
    }
}