using Microsoft.AspNetCore.Mvc;
using Models.Interfaces;
using Services.Interfaces;

namespace AppMvc.Models.ViewModels.Friends
{
    public class FriendsListViewModel
    {
        private readonly IFriendsService _friendsService;

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

        public void UpdatePagination(int nrOfItems)
        {
            //Pagination
            NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
            PrevPageNr = Math.Max(0, ThisPageNr - 1);
            NextPageNr = Math.Min(NrOfPages - 1, ThisPageNr + 1);
            NrVisiblePages = Math.Min(10, NrOfPages);
        }

        public FriendsListViewModel(IFriendsService friendsService)
        {
            _friendsService = friendsService;
        }
    }
}