using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using StudentManagement.Models;

namespace StudentManagement.ViewModels
{
    public class StudentCreateViewModel
    {
        public int Id { get; set; }

        [Display(Name = "姓名")]
        [Required(ErrorMessage = "请输入名字"), MaxLength(50, ErrorMessage = "名字的长度不能超过 50 个字符")]
        public string Name { get; set; }

        [Display(Name = "班级信息")]
        [Required(ErrorMessage = "请选择班级")]
        public ClassNameEnum? ClassName { get; set; }

        [Display(Name = "邮箱地址")]
        [Required(ErrorMessage = "请输入邮箱")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }

        [Display(Name = "图片")]
        public IFormFile Photo { get; set; }
    }
}