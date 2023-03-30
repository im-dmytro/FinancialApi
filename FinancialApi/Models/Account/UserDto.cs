using System.ComponentModel.DataAnnotations;

namespace FinancialApi.Models.Account
{
    public class UserDto : UserLogin
    {

        [Required, EmailAddress]
        public string Email { get; set; }


    }
    public class UserLogin
    {
        [Required, StringLength(15, MinimumLength = 6)]
        public string Username { get; set; } = string.Empty;

        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{6,15}$", ErrorMessage = "Minimum 6 and maximum 15 letters for password at least 1 letter and 1 number")]
        public string Password { get; set; } = string.Empty;
    }
}
