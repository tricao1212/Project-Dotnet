using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class RegisterInput
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public BookUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<BookUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(BookUser)}'. " +
                    $"Ensure that '{nameof(BookUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
    }
}
