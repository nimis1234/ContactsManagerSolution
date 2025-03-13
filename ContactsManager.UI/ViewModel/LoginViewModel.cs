using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContactsManager.UI.ViewModel
{
    public class LoginViewModel
    {

        [Required]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
