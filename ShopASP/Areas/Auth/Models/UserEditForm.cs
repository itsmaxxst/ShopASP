using System.ComponentModel.DataAnnotations;

namespace ShopASP.Areas.Auth.Models
{
    public class UserEditForm
    {
        public List<string>? UserRoles { get; set; }

        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }
    }
}
