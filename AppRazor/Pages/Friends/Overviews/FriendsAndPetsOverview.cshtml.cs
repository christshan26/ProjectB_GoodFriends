using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace AppRazor.Pages.Friends.Overviews
{
    public class FriendsAndPetsOverviewModel : PageModel
    {
        private readonly IAdminService _adminService;
        public IEnumerable<Models.DTO.GstUsrInfoFriendsDto>? CountryInfo;
        public IEnumerable<Models.DTO.GstUsrInfoPetsDto>? PetsInfo;

        public async Task<IActionResult> OnGet()
        {
            var friendsInfo = await _adminService.GuestInfoAsync();
            var petsInfo = await _adminService.GuestInfoAsync();

            CountryInfo = friendsInfo.Item.Friends
                .Where(f => f.City == null && f.Country != null);

            PetsInfo = petsInfo.Item.Pets
                .Where(p => p.City == null && p.Country != null);

            return Page();
        }

        public FriendsAndPetsOverviewModel(IAdminService adminService)
        {
            _adminService = adminService;
        }
    }
}
