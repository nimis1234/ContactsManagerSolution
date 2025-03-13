using ContactManager.Core.Domain.IdentityEntity;
using ContactManager.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactManager.Core.Services
{
    public class AccountServices : IAccountServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICustomEmailService _emailService;

        public AccountServices(UserManager<ApplicationUser> userManager, ICustomEmailService emailService)
        {
            _userManager=userManager;
            _emailService = emailService;
        }

        public async Task<ApplicationUser> GenerateOtpToken(ApplicationUser user)
        {
            // Generate OTP using Identity’s token provider
            // var otpCode = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "EmailOTP");

            // Generate OTP (6 digits) using a random number generator
            var otpCode = new Random().Next(100000, 999999).ToString();
            // Store OTP in the database for reference
            user.OTP = otpCode;
            user.OTPGeneratedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Send OTP via email
            var message = $"Thank you for registering. Your OTP is: {otpCode}. Use this to verify your account.";
            await _emailService.SendEmailAsync(user.Email, "Registration Verification", message);

            return user;
        }

        public async Task<bool> VerfiyUserOtp(string email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {

                if (user.OTP == otp)
                {
                    // Optionally, check if the OTP has expired (e.g., expired after 15 minutes)
                    if (user.OTPGeneratedAt.HasValue && DateTime.UtcNow - user.OTPGeneratedAt.Value > TimeSpan.FromMinutes(15))
                    {
                        return false; // OTP expired
                    }

                    // Clear OTP after successful verification
                    user.OTP = null;
                    user.OTPGeneratedAt = null;
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    return true;
                }

            }
            return false;
        }
    }
}
