using System.ComponentModel.DataAnnotations;

namespace StudentManagement.CustomeUtil
{
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        private readonly string _allowedDomain;

        public ValidEmailDomainAttribute(string allowedDomain)
        {
            _allowedDomain = allowedDomain;
        }

        public override bool IsValid(object value)
        {
            var strings = value.ToString().Split('@');

            return strings[1].ToUpper() == _allowedDomain.ToUpper();
        }
    }
}