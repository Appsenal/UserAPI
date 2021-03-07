using System.Linq;
using System.ComponentModel.DataAnnotations;
using UserAPI.Models;

namespace UserAPI.Utility
{
    public class UniqueEmail : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _context = (UserAPIContext)validationContext.GetService(typeof(UserAPIContext));
            //[pqa] check if there is one or more email from the datasource. 
            if (value != null)
            {
                var email = _context.Users.SingleOrDefault(e => e.Email == value.ToString());

                if (email != null)
                {
                    //[pqa] A duplicate is found.
                    return new ValidationResult(GetErrorMessage(value.ToString()));
                }
            }
            return ValidationResult.Success;
        }

        public string GetErrorMessage(string email)
        {
            return $"Email {email} is not available or already in use.";
        }
    }
}
