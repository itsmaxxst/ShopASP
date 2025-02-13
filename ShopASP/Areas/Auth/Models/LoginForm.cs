using System.ComponentModel.DataAnnotations;

namespace ShopASP.Areas.Auth.Models
{
    public class LoginForm
    {
        [EmailAddress(ErrorMessage = "Login must be an e-mail")]
        [Required(ErrorMessage ="Login is required")]
        [Display(Name = "Login")]
        public string Login {  get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
        public string Password {  get; set; }

    }
}
