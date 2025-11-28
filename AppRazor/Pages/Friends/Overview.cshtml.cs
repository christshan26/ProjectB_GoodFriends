using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services.Interfaces;

namespace MyApp.Namespace
{
    public class OverviewModel : PageModel
    {
        private readonly IAddressesService _addressesService;
        private readonly IAdminService _adminService;
        public IEnumerable<Models.DTO.GstUsrInfoFriendsDto>? CountryInfo;

        public async Task<IActionResult> OnGet()
        {
            var addresses = await _addressesService.ReadAddressesAsync(true, false, "Denmark", 0, 10);
            var info = await _adminService.GuestInfoAsync();

            CountryInfo = info.Item.Friends.Where(f => f.Country == "Denmark");
            return Page();
        }

        public OverviewModel(IAddressesService addressesService, IAdminService adminService)
        {
            _addressesService = addressesService;
            _adminService = adminService;
        }
    }
}
