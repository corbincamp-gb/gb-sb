using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using SkillBridge_System_Prototype.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Skillbridge.Business.Data;

namespace SkillBridge_System_Prototype.Controllers
{
    public class CmsController : Controller
    {
        internal readonly RoleManager<IdentityRole> _roleManager;
        internal readonly UserManager<ApplicationUser> _userManager;
        internal readonly ApplicationDbContext _db;

        public CmsController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                ViewBag.User = user;
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}