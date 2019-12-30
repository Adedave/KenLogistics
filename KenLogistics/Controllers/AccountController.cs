using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hangfire;
using KenLogistics.Data.Entities;
using KenLogistics.Infrastructure.IServices;
using KenLogistics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KenLogistics.Web.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        //private readonly OAuthConfig _oAuthConfig;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly IViewRenderService _viewRenderService;

        public AccountController(UserManager<ApplicationUser> userMgr, IEmailService emailService,
        SignInManager<ApplicationUser> signinMgr, IViewRenderService viewRenderService)
        //OAuthConfig oAuthConfig)
        {
            _userManager = userMgr;
            _emailService = emailService;
            _signInManager = signinMgr;
            _viewRenderService = viewRenderService;
            //_oAuthConfig = oAuthConfig;
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    LockoutEnabled = true
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    await AddUserToUsersRoleAsync(user.Email);

                    string cTokenLink = await GenerateEmailTokenAsync(user.Email);

                    await SendConfirmationEmailAsync(user.Email, cTokenLink);

                    //ViewBag.ConfirmEmail = "Registration successful, kindly check your email to confirm your registration"; /*: "";*/
                    //return RedirectToAction("Activate", new { email = user.Email });
                    TempData["Message"] = "Registration successful, kindly check your email to confirm your registration";
                    return RedirectToAction("Register");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Activate(string email)
        {
            var ApplicationUser = await _userManager.FindByEmailAsync(email);
            ViewBag.Email = ApplicationUser != null ? email : "<Email Not Found>";
            return View();
        }
        private async Task AddUserToUsersRoleAsync(string userEmail)
        {
            ApplicationUser registeredUser = await _userManager.FindByEmailAsync(userEmail);
            if (registeredUser != null)
            {
                IdentityResult result = await _userManager.AddToRoleAsync(registeredUser, "User");

                if (!result.Succeeded)
                {
                    AddErrorsFromResult(result);
                }
            }
        }

        private async Task SendConfirmationEmailAsync(string email, string cTokenLink)
        {
            var ApplicationUser = await _userManager.FindByEmailAsync(email);
            string message = await _viewRenderService.RenderToStringAsync("ConfirmEmailTemplate", email);
            message = message.Replace("{username}", ApplicationUser.UserName);
            message = message.Replace("{email}", ApplicationUser.Email);
            message = message.Replace("{confirmLink}", cTokenLink);
            var jobId = BackgroundJob.Enqueue(
                () => _emailService.ConfirmEmail(ApplicationUser.Email, message));
        }

        [AllowAnonymous]
        public async Task<IActionResult> ResendConfirmationEmailAsync(string email)
        {
            string confirmEmailTokenLink = await GenerateEmailTokenAsync(email);
            //bool success = await SendConfirmationEmailAsync(email, confirmEmailTokenLink);
            await SendConfirmationEmailAsync(email, confirmEmailTokenLink);
            return View("ConfirmEmail");
        }

        private async Task<string> GenerateEmailTokenAsync(string userEmail)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(userEmail);

            string cToken = _userManager.GenerateEmailConfirmationTokenAsync(user).Result;

            string cTokenLink = Url.Action("ConfirmEmail", "Account",
                values: new { userId = user.Id, token = cToken },
                protocol: HttpContext.Request.Scheme);
            return cTokenLink;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return View("Error", new ErrorViewModel());
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error", new ErrorViewModel());
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["Message"] = "This account has been activated. Kindly log in below";
                return RedirectToAction("Login");
            }
            return View("Error", new ErrorViewModel());
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(loginViewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel details, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByEmailAsync(details.Email);
                if (!user.EmailConfirmed && user.IsDeactivated == false)
                {
                    ViewBag.NotVerified = "You have not yet verified your account. Please check your mailbox for instructions on verifying your registration in order to log in";
                    return View(details);
                }
                if (user != null && user.IsDeactivated == false)
                {
                    await _signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result =
                        await _signInManager.PasswordSignInAsync(
                    user, details.Password, details.RememberMe, true);
                    var xc = HttpContext.User;
                    if (result.Succeeded)
                    {
                        return LocalRedirect(returnUrl ?? "/");
                    }
                    // If account is lockedout send the use to AccountLocked view
                    if (result.IsLockedOut)
                    {
                        TempData["Message"] = "Your account is locked, please try again after sometime or you may reset your password by clicking \"Forgot Your Password\" below";
                        return RedirectToAction("Login");
                    }
                }
                ModelState.AddModelError(nameof(LoginViewModel.Email), "Invalid user or password");
            }
            return View(details);
        }

        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            string redirectUrl = Url.Action("ExternalLoginCallback", "Account",
            new { ReturnUrl = returnUrl });
            var properties = _signInManager
            .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/", string remoteError = null)
        {
            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            if (remoteError != null)
            {
                ModelState.AddModelError("", $"Error from external provider: {remoteError}");
                return View("Login", loginViewModel);
            }
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError("", $"Error loading external login information");
                return View("Login", loginViewModel);
            }
            var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, false);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                var success = await ExternallRegister(info);
                if (success)
                {
                    return LocalRedirect(returnUrl);
                }
                ModelState.AddModelError("", $"Email claim not received from {info.LoginProvider}");
                ModelState.AddModelError("", $"Please contact support on support@expensetracker.com");
                return View("Login", loginViewModel);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalRegister()
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            var email = info.Principal.FindFirst(ClaimTypes.Email).Value;
            var firstName = info.Principal.FindFirst(ClaimTypes.GivenName).Value;
            var lastName = info.Principal.FindFirst(ClaimTypes.Surname).Value;

            RegisterViewModel registerViewModel = new RegisterViewModel
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };
            ModelState.AddModelError("", $"Email claim not received from {info.LoginProvider}");
            ModelState.AddModelError("", $"Please contact support on support@expensetracker.com");
            return View("Register", registerViewModel);
        }

        private async Task<bool> ExternallRegister(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirst(ClaimTypes.Email).Value;
            var firstName = info.Principal.FindFirst(ClaimTypes.GivenName).Value;
            var lastName = info.Principal.FindFirst(ClaimTypes.Surname).Value;
            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    IdentityResult identResult = await _userManager.AddLoginAsync(user, info);
                    if (identResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        return true;
                    }
                }
                else
                {
                    user = new ApplicationUser
                    {
                        Email = email,
                        UserName = email,
                        FirstName = firstName,
                        LastName = lastName
                    };
                    IdentityResult identityResult = await _userManager.CreateAsync(user);
                    if (identityResult.Succeeded)
                    {
                        await AddUserToUsersRoleAsync(user.Email);
                        identityResult = await _userManager.AddLoginAsync(user, info);
                        if (identityResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, false);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(email);

                //non-registered user, send no account registered email
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                //active users
                else if (user.IsDeactivated == false)
                {

                    // For more information on how to enable account confirmation and password reset please 
                    // visit https://go.microsoft.com/fwlink/?LinkID=532713
                    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetPasswordLink = Url.Action(
                         "ResetPassword", "Account",
                        values: new { email, token = resetToken },
                        protocol: Request.Scheme);

                    await SendResetPasswordEmail(email, resetPasswordLink);

                    return View("ForgotPasswordConfirmation");
                }

                //non-active users, send no account registered email
                return View("ForgotPasswordConfirmation");
            }
            //ModelState.AddModelError(nameof(email), "Email not found!");
            return View();
        }

        private async Task SendResetPasswordEmail(string email, string resetPasswordLink)
        {
            var ApplicationUser = await _userManager.FindByEmailAsync(email);
            string message = await _viewRenderService.RenderToStringAsync("ResetPasswordTemplate", email);
            message = message.Replace("{username}", ApplicationUser.UserName);
            message = message.Replace("{email}", ApplicationUser.Email);
            message = message.Replace("{resetPasswordLink}", resetPasswordLink);
            var jobId = BackgroundJob.Enqueue(
               () => _emailService.ResetPassword(ApplicationUser.Email, message));
        }

        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token = null)
        {
            if (token == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                ResetPasswordViewModel resetPassword = new ResetPasswordViewModel
                {
                    ResetPasswordToken = token,
                    Email = email
                };
                return View(resetPassword);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPassword)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(resetPassword.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return View("ResetPasswordConfirmation");
                }

                var result = await _userManager.ResetPasswordAsync(user, resetPassword.ResetPasswordToken, resetPassword.Password);
                if (result.Succeeded)
                {
                    // Upon successful password reset and if the account is lockedout, set
                    // the account lockout end date to current UTC date time, so the user
                    // can login with the new password
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                    }
                    return View("ResetPasswordConfirmation");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View();
        }

        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
