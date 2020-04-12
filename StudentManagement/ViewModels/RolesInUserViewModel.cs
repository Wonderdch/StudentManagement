using System.ComponentModel.DataAnnotations;

namespace StudentManagement.ViewModels
{
    public class RolesInUserViewModel
    {
        [Display(Name = "角色 Id")]
        public string RoleId { get; set; }

        [Display(Name = "角色名")]
        public string RoleName { get; set; }

        public bool IsSelected { get; set; }
    }
}