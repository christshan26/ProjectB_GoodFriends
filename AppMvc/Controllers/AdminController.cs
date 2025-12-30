using Microsoft.AspNetCore.Mvc;
using AppMvc.Models.ViewModels.Admin;
using Services.Interfaces;

namespace AppMvc.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IAdminService _adminService;

        public AdminController(ILogger<AdminController> logger, IAdminService adminService)
        {
            _logger = logger;
            _adminService = adminService;
        }

        [HttpGet]
        public IActionResult Seed()
        {
            var vm = new SeedViewModel();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Seed(SeedViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.RemoveSeeds)
                {
                    await _adminService.RemoveSeedAsync(true);
                    await _adminService.RemoveSeedAsync(false);
                }
                await _adminService.SeedAsync(model.NrOfItemsToSeed);
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
    }
}