using System.ComponentModel.DataAnnotations;

namespace StudentManagement.ViewModels
{
    public class EmailAddressViewModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}