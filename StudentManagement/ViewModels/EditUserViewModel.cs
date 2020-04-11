﻿using System.Collections.Generic;
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

        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [EmailAddress]
        [ValidEmailDomain(allowedDomain: "52abp.com", ErrorMessage = "邮箱后缀必须是 52abp.com")]
        public string Email { get; set; }

        public IList<string> Roles { get; set; }

        public List<string> Claims { get; set; }

        public string City { get; set; }
    }
}