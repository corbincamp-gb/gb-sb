using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SkillBridge.Business.Model.Db;

namespace SkillBridge.CMS.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return Redirect("/");
        }

    public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            await _signInManager.RefreshSignInAsync(user);

            
            var IUser = HttpContext.User;
            var identity = IUser.Identity as ClaimsIdentity;
            var claim = (from c in IUser.Claims where c.Type == ApplicationUser.MustChangePasswordClaimType select c).FirstOrDefault();
            //identity.RemoveClaim(claim);
            //bool claimRemoved = identity.TryRemoveClaim(claim);
            //Console.WriteLine("claimRemoved: " + claimRemoved);
            if (((ClaimsIdentity)HttpContext.User.Identity).HasClaim(c => c.Type == "http://userswithoutidentity/claims/mustchangepassword"))
            {
                var claimResult = await _userManager.RemoveClaimAsync(user, claim);
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}
