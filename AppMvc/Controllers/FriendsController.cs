using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppMvc.Models.ViewModels.Friends;
using Services.Interfaces;
using Models.DTO;
using AppMvc.SeidoHelpers;

namespace AppMvc.Controllers
{
    public class FriendsController : Controller
    {
        private readonly ILogger<FriendsController> _logger;
        private readonly IFriendsService _friendsService;
        private readonly IAddressesService _addressesService;
        private readonly IPetsService _petsService;
        private readonly IQuotesService _quotesService;

        public FriendsController(
            ILogger<FriendsController> logger,
            IFriendsService friendsService,
            IAddressesService addressesService,
            IPetsService petsService,
            IQuotesService quotesService
        )
        {
            _logger = logger;
            _friendsService = friendsService;
            _addressesService = addressesService;
            _petsService = petsService;
            _quotesService = quotesService;
        }

        [HttpGet]
        public async Task<IActionResult> List(int pagenr = 0, string searchFilter = null, bool useSeeds = true)
        {
            var vm = new FriendsListViewModel
            {
                ThisPageNr = pagenr,
                SearchFilter = searchFilter,
                UseSeeds = useSeeds
            };

            var resp = await _friendsService.ReadFriendsAsync(useSeeds, false, searchFilter, pagenr, vm.PageSize);
            vm.FriendsList = resp.PageItems;
            vm.NrOfFriends = resp.DbItemsCount;

            UpdatePagination(vm, resp.DbItemsCount);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Search(FriendsListViewModel model)
        {
            var resp = await _friendsService.ReadFriendsAsync(model.UseSeeds, false, model.SearchFilter, model.ThisPageNr, model.PageSize);
            model.FriendsList = resp.PageItems;
            model.NrOfFriends = resp.DbItemsCount;

            UpdatePagination(model, resp.DbItemsCount);
            return View("List", model);
        }

        private void UpdatePagination(FriendsListViewModel vm, int nrOfItems)
        {
            vm.NrOfPages = (int)Math.Ceiling((double)nrOfItems / vm.PageSize);
            vm.PrevPageNr = Math.Max(0, vm.ThisPageNr - 1);
            vm.NextPageNr = Math.Min(vm.NrOfPages - 1, vm.ThisPageNr + 1);
            vm.NrVisiblePages = Math.Min(10, vm.NrOfPages);
        }
    }
}