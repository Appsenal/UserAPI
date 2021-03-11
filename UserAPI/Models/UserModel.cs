using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Text.Json.Serialization;
using UserAPI.Utility;

namespace UserAPI.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserTypes : byte
    {
        Admin = 1,
        User = 2
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserStatus : byte
    {
        Active = 1,
        InActive = 2,
        Deleted = 3
    }

    //[pqa] Model use in the create request.
    public class UserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //[pqa] For autonumbering.
        public long Id { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [UniqueUserName]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "{0} should be 5 characters.")]
        //[MaxLength(5)]
        [RegularExpression(@"([a-zA-Z0-9]+)", ErrorMessage = "Invalid non-alphanumeric character(s).")]
        public string UserName { get; set; }

        //[JsonIgnore] //disable this if you want the password to display in the json response.
        [Required(ErrorMessage = "{0} is required")]
        //[MinLength(8, ErrorMessage = "{0} must be at least 8 characters")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.{8,}$)(?=.*?[a-z])(?=.*?[A-Z])(?=.*?[0-9])(?=.*?\W).*$", ErrorMessage = "{0} should at least 8 characters long, contain at least one number, both lower and uppercase letters and special characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [EmailAddress]
        [UniqueEmail]
        //[DataType(DataType.EmailAddress)]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Invalid {0}.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [EnumDataType(typeof(UserTypes))]
        public UserTypes UserType { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        //[JsonConverter(typeof(UserStatus))]
        [EnumDataType(typeof(UserStatus))]
        [DefaultValue(UserStatus.Active)]
        public UserStatus Status { get; set; } = UserStatus.Active;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedTime { get; set; }

        public string CreatedBy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime UpdatedTime { get; set; }

        public string UpdatedBy { get; set; }
    }

    //[pqa] Model use in the update request.
    public class UserModelUpdate
    {
        //public long Id { get; set; }

        [UniqueUserName]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "{0} should be 5 characters.")]
        [RegularExpression(@"([a-zA-Z0-9]+)", ErrorMessage = "Invalid non-alphanumeric character(s).")]
        public string UserName { get; set; }

        //[JsonIgnore] //[pqa] disable this if you want the password to display in the json response.
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.{8,}$)(?=.*?[a-z])(?=.*?[A-Z])(?=.*?[0-9])(?=.*?\W).*$", ErrorMessage = "{0} should at least 8 characters long, contain at least one number, both lower and uppercase letters and special characters.")]
        public string Password { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [EmailAddress]
        [UniqueEmail]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Invalid {0}.")]
        public string Email { get; set; }

        //[EnumDataType(typeof(UserTypes))]
        public UserTypes UserType { get; set; }

        //[EnumDataType(typeof(UserStatus))]
        //[DefaultValue(UserStatus.Active)]
        public UserStatus Status { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime UpdatedTime { get; set; }

        public string UpdatedBy { get; set; }
    }
}
