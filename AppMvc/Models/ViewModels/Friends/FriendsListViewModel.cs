using Microsoft.AspNetCore.Mvc;
using Models.Interfaces;

namespace AppMvc.Models.ViewModels.Friends
{
    public class FriendsListViewModel
    {
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
    }
}