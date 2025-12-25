using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Models.Interfaces;

namespace AppMvc.Models.ViewModels.Friends
{
    public class FriendsDetailsViewModel
    {
        private readonly IFriendsService _friendsService;
        private readonly IAddressesService _addressesService;
        public IFriend Friend { get; set; }
        public IAddress Address { get; set; }

        public FriendsDetailsViewModel(IFriendsService friendsService, IAddressesService addressesService)
        {
            _friendsService = friendsService;
            _addressesService = addressesService;
        }
    }
}