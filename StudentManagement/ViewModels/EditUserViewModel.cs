using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StudentManagement.CustomeUtil;

namespace StudentManagement.ViewModels
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            Claims = new List<string>();
            Roles = new List<string>();
        }

        [Display(Name = "用户 Id")]
        public string Id { get; set; }

        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [EmailAddress]
        [ValidEmailDomain(allowedDomain: "52abp.com", ErrorMessage = "邮箱后缀必须是 52abp.com")]
        [Display(Name = "邮箱")]
        public string Email { get; set; }

        public IList<string> Roles { get; set; }

        public List<string> Claims { get; set; }

        [Display(Name = "城市")]
        public string City { get; set; }
    }
}