using ASP_ITStep.Models;
using ASP_ITStep.Services.Identity;
using ASP_ITStep.Services.Random;
using ASP_ITStep.Services.Time;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ASP_ITStep.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITimeService _timeService;
        private readonly IRandomService _randomService;
        private readonly IIdentityService _identityService;

        public HomeController(ILogger<HomeController> logger, ITimeService timeService, IRandomService randomService, IIdentityService identityService)
        {
            _logger = logger;
            _timeService = timeService;
            _randomService = randomService;
            _identityService = identityService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Spa()
        {
            return View();
        }
        public IActionResult Consp()
        {
            return View();
        }
        public IActionResult Identity()
        {
            var ids = new List<long>();
            for (int i = 0; i < 5; i++)
            {
                ids.Add(_identityService.GenerateId());
            }
            ViewData["generatedIds"] = ids;
            return View();
        }
        public IActionResult Ioc()
        {
            ViewData["timestamp"] = _timeService.TimeStamp() + " -- " + _randomService.Otp();
            return View();
        }

        [HttpPost]
        public IActionResult GenerateNewId()
        {
            var newId = _identityService.GenerateId();
            var info = _identityService.GetIdInfo(newId);

            return Json(new { id = newId, info = info });
        }
        public IActionResult Razor()
        {
            HomeRazorPageModel model = new()
            {
                Arr = ["Item 1", "Item 2", "Item 3", "Item 4", "Item 5"]
            };
            return View(model);
        }

        public IActionResult Demo()
        {
            var product = new Product();
            return View(product);
        }


        public IActionResult Privacy()
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
