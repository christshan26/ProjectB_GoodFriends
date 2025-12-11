using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services;
using Services.Interfaces;


namespace AppRazor.Pages.Friends.Detailview
{
    public class ViewFriendModel : PageModel
    {
        private readonly IFriendsService _friendsService = null;
        private readonly IAddressesService _addressesService = null;
        public IFriend Friend { get; set; }
        public IAddress Address { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Guid _friendId = Guid.Parse(Request.Query["id"]);
            Friend = (await _friendsService.ReadFriendAsync(_friendId, false)).Item;

            return Page();
        }
    

        public ViewFriendModel(IFriendsService friendsService, IAddressesService addressesService)
        {
            _friendsService = friendsService;
            _addressesService = addressesService;
        }
    }
}