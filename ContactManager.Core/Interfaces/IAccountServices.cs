using ContactManager.Core.Domain.IdentityEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactManager.Core.Interfaces
{
    public interface IAccountServices
    {
        Task<bool> VerfiyUserOtp(string email, string otp);
        Task<ApplicationUser> GenerateOtpToken( ApplicationUser user);
    }
}
