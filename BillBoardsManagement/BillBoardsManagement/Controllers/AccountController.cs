using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Threading;
using BillBoardsManagement.Models;
using BillBoardsManagement.Repository;
using BillBoardsManagement.Common;
using System.Web.Security;
using System.IO;

namespace BillBoardsManagement.Controllers
{

    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var repository = new Repository<user>();
            var user = repository.GetAll().FirstOrDefault(x => x.Username == model.Username);
           
            if (user != null)
            {

                var password = EncryptionKeys.Encrypt(model.Password);
                if (user.Password.Equals(password))
                {

                    var role = new Repository<lk_role>().Get(user.RoleId);
                    var enumRole = (EnumUserRole)role.Code;
                    string route = Request.Form["route"];
                   
                    var cu = new ContextUser
                    {
                        OUser = user,
                        EnumRole = enumRole,
                        Role = role,
                        PhotoPath = "/img/avatars/admin.png"
                    };

                    Session["user"] = cu;
                    FormsAuthentication.SetAuthCookie(user.Username, false);
                   
                    return RedirectToPortal(enumRole, cu);
                }
            }

            ViewBag.Error = "Login data is incorrect!";
            return View(model);

            //// This doesn't count login failures towards account lockout
            //// To enable password failures to trigger account lockout, change to shouldLockout: true
            //var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            //switch (result)
            //{
            //    case SignInStatus.Success:
            //        return RedirectToLocal(returnUrl);
            //    case SignInStatus.LockedOut:
            //        return View("Lockout");
            //    case SignInStatus.RequiresVerification:
            //        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
            //    case SignInStatus.Failure:
            //    default:
            //        ModelState.AddModelError("", "Invalid login attempt.");
            //        return View(model);
            //}


        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }
         
        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

     
        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Request.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Login");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectToPortal(EnumUserRole userRole, ContextUser user)
        {
            switch (userRole)
            {
                case EnumUserRole.SuperAdmin:
                    return RedirectToAction("Index", "Customer");
                
                default:
                    return RedirectToAction("Index", "Home");
            }

        }
        public ActionResult UpdatePassword()
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            return View(model);
        }
        
        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        public ActionResult UserList(string sortOrder, string filter, string archived, int page = 1, Guid? archive = null)
        {

            ViewBag.searchQuery = string.IsNullOrEmpty(filter) ? "" : filter;
            ViewBag.showArchived = (archived ?? "") == "on";

            page = page > 0 ? page : 1;
            int pageSize = 0;
            pageSize = pageSize > 0 ? pageSize : 10;

            ViewBag.CurrentSort = sortOrder;

            List<user> users;
            var repository = new Repository<user>();
            if (archive != null)
            {
                var oUser = repository.GetByGuid(archive.Value);
                oUser.IsLocked = !oUser.IsLocked;
                repository.Put(oUser.Id, oUser);
            }
            int[] rols = { (int)EnumUserRole.SuperAdmin, (int)EnumUserRole.User};
            if (string.IsNullOrEmpty(filter))
            {
                users = repository.GetAll().Where(x => rols.Contains(x.lk_role.Code)).ToList();
            }
            else
            {
                users = repository.GetAll().Where(x => x.Username.ToLower().Contains(filter.ToLower()) && rols.Contains(x.lk_role.Code)).ToList();
            }
            //Sorting order
            users = users.OrderByDescending(x => x.CreatedAt).ToList();
            ViewBag.Count = users.Count();

            return View(users);
        }

        public ActionResult EditUser(Guid? id)
        {
           var repo = new Repository<user>();
            var reporole = new Repository<lk_role>();
            int[] rolesCode = { (int)EnumUserRole.SuperAdmin, (int)EnumUserRole.User};
            user us = null;

            ViewBag.rolesdd = reporole.GetAll().Where(x => rolesCode.Contains(x.Code)).Select(x =>
                   new SelectListItem { Text = x.Name, Value = x.Id + "" }
            ).ToList();
            if (id != null)
            {
                us = repo.FindAll(x=>x.RowGuid == id.Value).FirstOrDefault();
            }
            else
            {
                us = new user();
                us.Password = Membership.GeneratePassword(8, 2);
            }
            us.IsLocked = !us.IsLocked;
            return View(us);
        }
        [HttpPost]
        public ActionResult EditUser(user user, HttpPostedFileBase file)
        {
            var cu = Session["user"] as ContextUser;
            var repo = new Repository<user>();
            user oUser = null;
            if (user.Id == 0)
            {
                oUser = new user();
                oUser.RowGuid = Guid.NewGuid();
                oUser.CreatedAt = DateTime.Now;
                oUser.CreatedBy = cu.OUser.Id;
                oUser.Email = user.Email;
                oUser.Password = EncryptionKeys.Encrypt(user.Password);
                oUser.RegistrationDate = DateTime.Now;
                oUser.Username = user.Username;

            }
            else
            {
                oUser = repo.Get(user.Id);
                oUser.UpdatedBy = cu.OUser.Id;
                oUser.UpdatedAt = DateTime.Now;

            }
            oUser.FirstName = user.FirstName;
            oUser.LastName = user.LastName;
            oUser.RoleId = user.RoleId;
            oUser.IsLocked = !user.IsLocked;
            if (file != null)
            {
                string fileName = "~/Uploads/ImageLibrary/" + Guid.NewGuid() + Path.GetExtension(file.FileName);
                string filePath = Server.MapPath(fileName);
                file.SaveAs(filePath);
                oUser.PhotoPath = fileName;
            }
            if (oUser.Id > 0)
                repo.Put(oUser.Id, oUser);
            else
            {
                int[] rolesCode = { (int)EnumUserRole.SuperAdmin, (int)EnumUserRole.User };

                var reporole = new Repository<lk_role>();

                ViewBag.rolesdd = reporole.GetAll().Where(x => rolesCode.Contains(x.Code)).Select(x =>
                       new SelectListItem { Text = x.Name, Value = x.Id + "" }
                ).ToList();

                if (repo.GetAll().Where(x=>x.Username == oUser.Username).Any())
                {
                    ViewBag.userexist = true;
                    return View(user);
                }
                if (repo.GetAll().Where(x => x.Email == oUser.Email).Any())
                {
                    ViewBag.emailexist = true;
                    return View(user);
                }
                repo.Post(oUser);
            }
            return RedirectToAction("UserList");
        }
    }
}