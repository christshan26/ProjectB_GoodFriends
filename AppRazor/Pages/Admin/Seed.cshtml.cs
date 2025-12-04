using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;
using Services.Interfaces;

namespace AppRazor.Pages.Admin
{
    public class SeedModel : PageModel
    {
        readonly IAdminService _adminService = null;
        readonly ILogger<SeedModel> _logger = null;
        public int NrOfFriends => nrOfFriends().Result;
        private async Task<int> nrOfFriends()
        {
            var info = await _adminService.GuestInfoAsync();
            return info.Item.Db.NrSeededFriends + info.Item.Db.NrUnseededFriends;
        }

        [BindProperty]
        [Required(ErrorMessage = "You must enter nr of items to seed")]
        public int NrOfItemsToSeed { get; set; } = 100;

        [BindProperty]
        public bool RemoveSeeds { get; set; } = true;


        public IActionResult OnGet()
        {
            return Page();
        }
        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                if (RemoveSeeds)
                {
                    await _adminService.RemoveSeedAsync(true);
                    await _adminService.RemoveSeedAsync(false);
                }
                await _adminService.SeedAsync(NrOfItemsToSeed);
                return Redirect("~/Index");
            }
            return Page();
        }

        public SeedModel(IAdminService adminService, ILogger<SeedModel> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }
    }
}
