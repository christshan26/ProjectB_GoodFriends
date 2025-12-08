using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages.Friends
{
    public class FriendsAndPetsCityOverviewModel : PageModel
    {
        private readonly IAdminService _adminService;
        public IEnumerable<Models.DTO.GstUsrInfoFriendsDto>? CountryInfo;
        public IEnumerable<Models.DTO.GstUsrInfoFriendsDto>? CityInfo;
        public IEnumerable<Models.DTO.GstUsrInfoPetsDto>? PetsInfo;

        [BindProperty(SupportsGet = true)]
        public string? Country { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var friendsInfo = await _adminService.GuestInfoAsync();
            var petsInfo = await _adminService.GuestInfoAsync();

            CountryInfo = friendsInfo.Item.Friends
                .Where(f => f.City == null && f.Country == Country);

            CityInfo = friendsInfo.Item.Friends
                .Where(f => f.City != null && f.Country == Country);

            PetsInfo = petsInfo.Item.Pets
                .Where(p => p.City != null && p.Country == Country);

            return Page();
        }

        public FriendsAndPetsCityOverviewModel(IAdminService adminService)
        {
            _adminService = adminService;
        }
    }
}
