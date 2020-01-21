using System.ComponentModel.DataAnnotations;

namespace StudentManagement.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "邮箱地址不能为空")]
        [EmailAddress]
        [Display(Name = "邮箱地址")]
        public string Email { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我")]
        public bool RememberMe { get; set; }
    }
}