using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentManagement.Models;
using StudentManagement.ViewModels;

namespace StudentManagement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger _logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    City = model.City
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // 生成电子邮件确认令牌
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // 生成电子邮件的确认链接
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token }, Request.Scheme);

                    _logger.Log(LogLevel.Warning, confirmationLink);

                    if (_signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Admin");
                    }

                    ViewBag.ErrorTitle = "注册成功";
                    ViewBag.ErrorMessage = "在你登入系统前，我们已经给您发了一份邮件，需要您先进行邮件验证，点击确认链接即可完成。";
                    return View("Error");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.EmailConfirmed &&
                    (await _userManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "您的电子邮箱尚未进行验证。");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    // 防止 Open Redirect
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "登录失败，请重试");
            }

            return View(model);
        }

        #region 扩展登录

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            var loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"外部提供程序错误: {remoteError}");

                return View("Login", loginViewModel);
            }

            // 从外部登录提供者,即微软账户体系中，获取关于用户的登录信息。
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "加载外部登录信息出错。");

                return View("Login", loginViewModel);
            }

            ApplicationUser user = null;

            // 获取邮箱地址
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email != null)
            {
                // 通过邮箱地址去查询用户是否已存在
                user = await _userManager.FindByEmailAsync(email);

                // 如果电子邮件没有被确认，返回登录视图与验证错误
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "您的电子邮箱尚未进行验证。");

                    return View("Login", loginViewModel);
                }
            }

            // 如果用户之前已经登录过了，会在 AspNetUserLogins 表有对应的记录，这个时候无需创建新的记录，直接使用当前记录登录系统即可。
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
                isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            // 如果 AspNetUserLogins 表中没有记录，则代表用户没有一个本地帐户，这个时候我们就需要创建一个记录了。
            if (email != null)
            {
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };

                    //如果不存在，则创建一个用户，但是这个用户没有密码。
                    await _userManager.CreateAsync(user);

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token },
                        Request.Scheme);

                    _logger.Log(LogLevel.Warning, confirmationLink);
                    ViewBag.ErrorTitle = "注册成功";
                    ViewBag.ErrorMessage = "在你登入系统前，我们已经给您发了一份邮件，需要您先进行邮件验证，点击确认链接即可完成。";
                    return View("Error");
                }

                // 在 AspNetUserLogins 表中,添加一行用户数据，然后将当前用户登录到系统中
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);

                return LocalRedirect(returnUrl);
            }

            // 如果我们获取不到电子邮件地址，我们需要将请求重定向到错误视图中。
            ViewBag.ErrorTitle = $"我们无法从提供商:{info.LoginProvider}中解析到您的邮件地址 ";
            ViewBag.ErrorMessage = "请通过联系 ltm@ddxc.org 寻求技术支持。";

            return View("Error");
        }

        #endregion

        #region 确认邮箱

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("index", "home");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"当前{userId}无效";
                return View("NotFound");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return View();
            }

            ViewBag.ErrorTitle = "您的电子邮箱尚未进行验证。";
            return View("Error");
        }

        #endregion 确认邮箱

        #region 激活邮箱

        [HttpGet]
        public IActionResult ActivateUserEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ActivateUserEmail(EmailAddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                // 当前已经存在老用户时    1.生成电子令牌    2.以及电子令牌确认URL
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        // 生成电子邮件确认令牌
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                        // 生成电子邮件的确认链接
                        var confirmationLink = Url.Action("ConfirmEmail", "Account",
                            new { userId = user.Id, token }, Request.Scheme);

                        _logger.Log(LogLevel.Warning, confirmationLink);
                        ViewBag.Message = "如果你在我们系统有注册账户，我们已经发了邮件到您的邮箱中，请前往邮箱激活您的用户。";

                        // 重定向用户到忘记密码确认视图
                        return View("ActivateUserEmailConfirmation", ViewBag.Message);
                    }
                }

                ViewBag.Message = "请确认邮箱是否存在异常，现在我们无法给您发送激活链接。";

                // 为了避免帐户枚举和暴力攻击，所以不进行用户不存在或邮箱未验证的提示
                return View("ActivateUserEmailConfirmation", ViewBag.Message);
            }

            return View();
        }

        #endregion 激活邮箱

        #region 找回密码 & 重置密码

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(EmailAddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 通过邮件地址查询用户地址
                var user = await _userManager.FindByEmailAsync(model.Email);

                // 如果找到了用户并且确认了电子邮件
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    // 生成重置密码令牌
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    // 生成密码重置链接
                    var passwordResetLink = Url.Action("ResetPassword", "Account",
                            new { email = model.Email, token }, Request.Scheme);

                    // 将密码重置链接记录到文件中
                    _logger.Log(LogLevel.Warning, passwordResetLink);

                    // 重定向用户到忘记密码确认视图
                    return View("ForgotPasswordConfirmation");
                }

                // 为了避免帐户穷举和暴力攻击，所以不进行用户不存在或邮箱未验证的提示
                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        // 电子邮箱，重置密码的 token，新密码，确认密码
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            // 如果密码的 token 或者邮箱地址为空，用户有可能在试图篡改密码重置的 URL
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "当前的密码重置令牌无效");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 通过电子邮件查找用户
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // 重置用户密码
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                    if (result.Succeeded)
                    {
                        // 密码成功重置后，如果当前账户被锁定，则设置该账户锁定结束日期为当前 UTC 日期时间。
                        // 这样用户就可以用新密码登录系统。
                        if (await _userManager.IsLockedOutAsync(user))
                        {
                            // DateTimeOffset 指 UTC 时间即格林威治时间
                            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                        }
                        return View("ResetPasswordConfirmation");
                    }

                    // 告诉它验证不通过的错误信息
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // 为了避免帐户穷举和暴力攻击，不要提示用户不存在
                return View("ResetPasswordConfirmation");
            }
            // 如果模型验证未通过，则显示验证错误
            return View(model);
        }

        #endregion 找回密码 & 重置密码

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            return user == null ? Json(true) : Json($"邮箱：{email} 已经被注册使用了。");
        }
    }
}