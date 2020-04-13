using System.Collections.Generic;
using StudentManagement.Models;

namespace StudentManagement.ViewModels
{
    public class UserClaimsViewModel
    {
        public string UserId { get; set; }

        public List<UserClaim> Claims { get; set; }

        public UserClaimsViewModel()
        {
            Claims = new List<UserClaim>();
        }
    }
}