using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;

namespace AppRazor.Pages.Friends.Lists
{
    public class FriendsListModel : PageModel
    {
        private readonly Services.Interfaces.IFriendsService _friendsService = null;

        [BindProperty]
        public bool UseSeeds { get; set; } = true;

        public List<IFriend> FriendsList {get; set;}
        public int NrOfFriends { get; set; }

        public int NrOfPages { get; set; }
        public int PageSize { get; set; } = 10;

        public int ThisPageNr { get; set; } = 0;
        public int PrevPageNr { get; set; } = 0;
        public int NextPageNr { get; set; } = 0;
        public int NrVisiblePages { get; set; } = 0;

        [BindProperty]
        public string SearchFilter { get; set; } = null;

        public async Task<IActionResult> OnGet()
        {
            if (int.TryParse(Request.Query["pagenr"], out int pagenr))
            {
                ThisPageNr = pagenr;
            }

            SearchFilter = Request.Query["search"];

            var resp = await _friendsService.ReadFriendsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            FriendsList = resp.PageItems;
            NrOfFriends = resp.DbItemsCount;

            UpdatePagination(resp.DbItemsCount);

            return Page();
        }

        private void UpdatePagination(int nrOfItems)
        {
            NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
            PrevPageNr = Math.Max(0, ThisPageNr - 1);
            NextPageNr = Math.Min(NrOfPages - 1, ThisPageNr + 1);
            NrVisiblePages = Math.Min(10, NrOfPages);
        }

        public async Task<IActionResult> OnPostSearch()
        {
            var resp = await _friendsService.ReadFriendsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            FriendsList = resp.PageItems;
            NrOfFriends = resp.DbItemsCount;

            UpdatePagination(resp.DbItemsCount);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteFriend(Guid id)
        {
            await _friendsService.DeleteFriendAsync(id);

            var resp = await _friendsService.ReadFriendsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            FriendsList = resp.PageItems;
            NrOfFriends = resp.DbItemsCount;

            UpdatePagination(resp.DbItemsCount);

            return Page();
        }

        public FriendsListModel(Services.Interfaces.IFriendsService friendsService)
        {
            _friendsService = friendsService;
        }
    }
}
