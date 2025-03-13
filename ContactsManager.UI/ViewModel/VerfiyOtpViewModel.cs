using System.ComponentModel.DataAnnotations;

namespace ContactsManager.UI.ViewModel
{
    public class VerfiyOtpViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [Display(Name = "OTP Code")]
        public string OTP { get; set; }
    }
}
