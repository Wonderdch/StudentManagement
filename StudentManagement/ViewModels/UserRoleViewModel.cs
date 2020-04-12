using System.ComponentModel.DataAnnotations;

namespace StudentManagement.ViewModels
{
    public class UserRoleViewModel
    {
        [Display(Name = "用户 Id")]
        public string UserId { get; set; }

        [Display(Name = "用户名")]
        public string UserName { get; set; }

        public bool IsSelected { get; set; }
    }
}