using Models.Interfaces;

namespace AppMvc.Models.ViewModels.Friends
{
    public class FriendsDetailsViewModel
    {
        public IFriend Friend { get; set; }
        public IAddress Address { get; set; }

    }
}