using System.ComponentModel.DataAnnotations;

namespace ShopASP.Areas.Auth.Models
{
    public class RegisterForm
    {
		[EmailAddress(ErrorMessage = "Login must be an e-mail")]
		[Required(ErrorMessage = "Login is required")]
		[Display(Name = "Login (e-mail)")]
		public string Login { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Passwords are not matching")]
		[Display(Name = "Confirm Password")]
		public string ConfirmPassword { get; set; }
	}
}
