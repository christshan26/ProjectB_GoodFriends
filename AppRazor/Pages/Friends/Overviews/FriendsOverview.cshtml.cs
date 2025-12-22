using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace AppRazor.Pages.Friends.Overviews
{
    public class FriendsOverviewModel : PageModel
    {
        private readonly IAdminService _adminService;
        public IEnumerable<Models.DTO.GstUsrInfoFriendsDto>? CountryInfo;

        public async Task<IActionResult> OnGet()
        {
            var info = await _adminService.GuestInfoAsync();

            CountryInfo = info.Item.Friends
                .Where(f => f.City == null && f.Country != null);

            return Page();
        }

        public FriendsOverviewModel(IAdminService adminService)
        {
            _adminService = adminService;
        }
    }
}
