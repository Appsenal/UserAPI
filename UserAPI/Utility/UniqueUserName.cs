using System.Linq;
using System.ComponentModel.DataAnnotations;
using UserAPI.Models;

namespace UserAPI.Utility
{
    public class UniqueUserName : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _context = (UserAPIContext)validationContext.GetService(typeof(UserAPIContext));
            //[pqa] check if there is one or more email from the datasource. 
            if (value != null)
            {
                var userName = _context.Users.SingleOrDefault(e => e.UserName == value.ToString());

                if (userName != null)
                {
                    //[pqa] A duplicate is found.
                    return new ValidationResult(GetErrorMessage(value.ToString()));
                }
            }
            return ValidationResult.Success;
        }

        public string GetErrorMessage(string username)
        {
            return $"Username {username} is already in use.";
        }
    }
}
