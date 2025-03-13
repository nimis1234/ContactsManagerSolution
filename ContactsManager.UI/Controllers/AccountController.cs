using ContactManager.Core.Domain.IdentityEntity;
using ContactManager.Core.Interfaces;
using ContactManager.Core.Services;
using ContactsManager.UI.ViewModel;
using CURDExample.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using NETCore.MailKit.Infrastructure.Internal;
using System.Security.Claims;

namespace ContactsManager.UI.Controllers
{
    [Route("Account")]
    [AllowAnonymous]
   // [Authorize(Policy = "Unauthenticated")] // instead of allowanonymous, we can use custom policy
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAccountServices _accountServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RoleManager<ApplicationRole> _roleManager;


        // in order to create login, register etc  via identity , use user manager, role manager, and sign in manager

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager ,
            IAccountServices accountServices,
            IHttpContextAccessor httpContextAccessor,
            RoleManager<ApplicationRole> roleManager
            )
        {
            _userManager=userManager;
            _signInManager=signInManager;
            _accountServices = accountServices;
            _httpContextAccessor = httpContextAccessor;
            _roleManager=roleManager;
        }


     
        [Route("Register")]
        [HttpGet]
        
        public async Task<IActionResult> Register()
        {
            // show the registration page

            return View();
        }

        [Route("Register")]
        [HttpPost]
        [ModalValidationFilterFactory(2)]
        public async Task<IActionResult> Register(RegistrationViewModel RegistrationAdd)
        {
            //

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = RegistrationAdd.Email,
                    Email = RegistrationAdd.Email,
                    FirstName = RegistrationAdd.FirstName,
                    LastName = RegistrationAdd.LastName,
                    ProfilePicture = null
                };
                var result = await _userManager.CreateAsync(user, RegistrationAdd.Password); ; //createAsync is a method that comes from user manager (identity framework)
                if (result.Succeeded)
                {
                    // user created successfully
                    //generate OTP and send as email

                    // for now user can only register

                    await _userManager.AddToRoleAsync(user, "User");
                    ApplicationUser updatedUser = await _accountServices.GenerateOtpToken(user);


                    return RedirectToAction("VerifyOtp", "Account", new { email = updatedUser.Email });
                }
                else
                {
                    // add the errors to the model state
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
               
            return View();
        }


        [HttpGet]
        [Route("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }
            // use VerifyOtpViewModel
            VerfiyOtpViewModel verfiyOtpViewModel = new VerfiyOtpViewModel()
            {
                Email = user.Email,
                OTP = user.OTP
            };
            return View(verfiyOtpViewModel);
        }




        [HttpPost]
        [Route("VerifyOtpEntered")]
        public async Task<IActionResult> VerifyOtpEntered( VerfiyOtpViewModel verfiyOtpViewModel)
        {
            var email = verfiyOtpViewModel.Email;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }
           
            // update the verify otp result call service to do this

             var isValid = await _accountServices.VerfiyUserOtp(user.Email, verfiyOtpViewModel.OTP);
            if (isValid)
            {
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return BadRequest("Invalid OTP");
            }
            
        }

        //logout
        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            var getCurrentUserEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email); // the email of the current user
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        [Route("Login")]

        public async Task<IActionResult> Login(string returnUrl = null)
        {
            ViewBag.returnUrl = returnUrl;
            var getCurrentUserEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email); // the email of the current user
            return View();
        }

        [HttpPost]
        [Route("Login")]
        [ModalValidationFilterFactory(2)]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel,string ReturnUrl = null)
        {
            ApplicationUser user; ;
            if(ModelState.IsValid)
            {
                // check user email and password validation
                var result = await _signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, false, false);//isPersistent: false, // Session-based login
                if(!result.Succeeded)
                {
                    // add the errors to the model state     
                    ModelState.AddModelError("", "Invalid login attempt.");
                   return View();
                }

                user = await _userManager.FindByEmailAsync(loginViewModel.Email);
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if(isAdmin)
                {
                    return RedirectToAction("AdminIndex", "Admin", new { area = "Admin" }); //will redirect to admin area
                }


                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    ViewBag.ReturnUrl = ReturnUrl;
                    return Redirect(ReturnUrl);
                }

                return RedirectToAction("Index", "Persons");
            }

            return View();
        }


        // isEmailAvaliable
       // remote validation
        public async Task<IActionResult> IsEmailAvailable(string email)
        {
            bool exists = await _userManager.FindByEmailAsync(email) != null;
            return Json(exists ? "Email is already in use." : true);
        }
    }
}
