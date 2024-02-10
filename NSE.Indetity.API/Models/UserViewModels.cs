using System.ComponentModel.DataAnnotations;

namespace NSE.Indetity.API.Models
{
    public class UserRegister
    {
        [Required(ErrorMessage = "the field {0} is mandatory")]
        [EmailAddress(ErrorMessage = "the field {0} is in an invalid format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "the field {0} is mandatory")]
        [StringLength(100, ErrorMessage = "the field must be between {2} and {1} characters", MinimumLength = 6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "passwords don't match")]
        public string PasswordConfirmation { get; set; }
    }


    public class UserLogin
    {
        [Required(ErrorMessage = "the field {0} is mandatory")]
        [EmailAddress(ErrorMessage = "the field {0} is in an invalid format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "the field {0} is mandatory")]
        [StringLength(100, ErrorMessage = "the field must be between {2} and {1} characters", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
