using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactManager.Core.Domain.IdentityEntity
{
    public class ApplicationUser : IdentityUser<Guid> //: IdentityUser is a class that represents a user in the identity system 
        // this needs to install Microsoft.AspNetCore.Identity.stores
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public byte[]? ProfilePicture { get; set; } // For photo capture

        public string? OTP { get; set; } // Store OTP temporarily
        public DateTime? OTPGeneratedAt { get; set; }
    }
}
