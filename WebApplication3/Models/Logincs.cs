using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Logincs
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string PassWord { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool LoggedStatus { get; set; }
    }
}
