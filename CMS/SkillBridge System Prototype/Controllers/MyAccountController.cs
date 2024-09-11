using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkillBridge_System_Prototype.ViewModel;
using Skillbridge.Business.Model.Db;

namespace SkillBridge_System_Prototype.Controllers
{
    [Authorize]

    public class MyAccountController : Controller
    {
        private readonly ILogger<MyAccountController> _logger;

        public MyAccountController(ILogger<MyAccountController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
