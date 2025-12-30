using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppMvc.Models.ViewModels.Friends;
using Services.Interfaces;
using Models.DTO;
using AppMvc.SeidoHelpers;
using Models.Interfaces;
using static AppMvc.Models.ViewModels.Friends.EditFriendViewModel;

namespace AppMvc.Controllers
{
    public class FriendsController : Controller
    {
        private readonly ILogger<FriendsController> _logger;
        private readonly IFriendsService _friendsService;
        private readonly IAddressesService _addressesService;
        private readonly IPetsService _petsService;
        private readonly IQuotesService _quotesService;
        private readonly IAdminService _adminService;


        public FriendsController(
            ILogger<FriendsController> logger,
            IFriendsService friendsService,
            IAddressesService addressesService,
            IPetsService petsService,
            IQuotesService quotesService,
            IAdminService adminService
        )
        {
            _logger = logger;
            _friendsService = friendsService;
            _addressesService = addressesService;
            _petsService = petsService;
            _quotesService = quotesService;
            _adminService = adminService;
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

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id, FriendsListViewModel model)
        {
            await _friendsService.DeleteFriendAsync(id);

            var resp = await _friendsService.ReadFriendsAsync(model.UseSeeds, false, model.SearchFilter, model.ThisPageNr, model.PageSize);

            model.FriendsList = resp.PageItems;
            model.NrOfFriends = resp.DbItemsCount;

            UpdatePagination(model, resp.DbItemsCount);
            return View("List", model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var friend = await _friendsService.ReadFriendAsync(id, false);

            var vm = new FriendsDetailsViewModel
            {
                Friend = friend.Item
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            var vm = new EditFriendViewModel();
            vm.AnimalKind = new List<SelectListItem>().PopulateSelectList<AnimalKind>();
            vm.ValidationResult = new ModelValidationResult(false, null, null);

            if (id.HasValue)
            {
                var fr = await _friendsService.ReadFriendAsync(id.Value, false);
                vm.FriendInput = new FriendIM(fr.Item);
                vm.PageHeader = "Edit details for a friend";
            }
            else
            {
                vm.FriendInput = new FriendIM();
                vm.FriendInput.StatusIM = StatusIM.Inserted;
                vm.PageHeader = "Create new friend";
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditAddress(EditFriendViewModel model)
        {
            string[] keys = { "FriendInput.Address.StreetAddress", "FriendInput.Address.ZipCode", "FriendInput.Address.City", "FriendInput.Address.Country" };

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                model.ValidationResult = validationResult;
                return View("Edit", model);
            }

            if (model.FriendInput.Address.AddressId == null || model.FriendInput.Address.AddressId == Guid.Empty)
            {
                model.FriendInput.Address.StatusIM = StatusIM.Inserted;
                model.FriendInput.Address.AddressId = Guid.NewGuid();
            }
            else if (model.FriendInput.Address.StatusIM != StatusIM.Inserted)
            {
                model.FriendInput.Address.StatusIM = StatusIM.Modified;
            }

            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> Undo(EditFriendViewModel model)
        {
            var fr = await _friendsService.ReadFriendAsync(model.FriendInput.FriendId, false);
            model.FriendInput = new FriendIM(fr.Item);
            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAddress(EditFriendViewModel model)
        {
            if (model.FriendInput.Address.AddressId != null)
            {
                model.FriendInput.Address.StatusIM = StatusIM.Deleted;
            }

            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddPet(EditFriendViewModel model)
        {
            string[] keys = { "FriendInput.NewPet.Name", "FriendInput.NewPet.Kind" };

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                model.ValidationResult = validationResult;
                return View("Edit", model);
            }

            model.FriendInput.NewPet.StatusIM = StatusIM.Inserted;
            model.FriendInput.NewPet.PetId = Guid.NewGuid();
            model.FriendInput.Pets.Add(new PetIM(model.FriendInput.NewPet));
            model.FriendInput.NewPet = new PetIM();

            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditPet(Guid petId, EditFriendViewModel model)
        {
            int idx = model.FriendInput.Pets.FindIndex(p => p.PetId == petId);
            string[] keys = { $"FriendInput.Pets[{idx}].editName", $"FriendInput.Pets[{idx}].editKind" };

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                model.ValidationResult = validationResult;
                return View("Edit", model);
            }

            var p = model.FriendInput.Pets.First(p => p.PetId == petId);
            if (p.StatusIM != StatusIM.Inserted)
            {
                p.StatusIM = StatusIM.Modified;
            }

            p.Name = p.editName;
            p.Kind = p.editKind;

            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePet(Guid petId, EditFriendViewModel model)
        {
            var pet = model.FriendInput.Pets.FirstOrDefault(p => p.PetId == petId);
            if (pet != null)
            {
                pet.StatusIM = StatusIM.Deleted;
            }
            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddQuote(EditFriendViewModel model)
        {
            string[] keys = { "FriendInput.NewQuote.QuoteText", "FriendInput.NewQuote.Author" };

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                model.ValidationResult = validationResult;
                return View("Edit", model);
            }

            model.FriendInput.NewQuote.StatusIM = StatusIM.Inserted;
            model.FriendInput.NewQuote.QuoteId = Guid.NewGuid();
            model.FriendInput.Quotes.Add(new QuoteIM(model.FriendInput.NewQuote));
            model.FriendInput.NewQuote = new QuoteIM();

            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditQuote(Guid quoteId, EditFriendViewModel model)
        {
            int idx = model.FriendInput.Quotes.FindIndex(q => q.QuoteId == quoteId);
            string[] keys = { $"FriendInput.Quotes[{idx}].editQuoteText", $"FriendInput.Quotes[{idx}].editAuthor" };

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                model.ValidationResult = validationResult;
                return View("Edit", model);
            }

            var q = model.FriendInput.Quotes.First(q => q.QuoteId == quoteId);
            if (q.StatusIM != StatusIM.Inserted)
            {
                q.StatusIM = StatusIM.Modified;
            }

            q.QuoteText = q.editQuoteText;
            q.Author = q.editAuthor;

            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuote(Guid quoteId, EditFriendViewModel model)
        {
            var quote = model.FriendInput.Quotes.FirstOrDefault(q => q.QuoteId == quoteId);
            if (quote != null)
            {
                quote.StatusIM = StatusIM.Deleted;
            }
            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(EditFriendViewModel model)
        {
            string[] keys = { "FriendInput.FirstName", "FriendInput.LastName" };

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                model.ValidationResult = validationResult;
                return View("Edit", model);
            }

            // Create new friend if this is an insert operation
            if (model.FriendInput.StatusIM == StatusIM.Inserted)
            {
                var newFr = await _friendsService.CreateFriendAsync(model.FriendInput.CreateCUdto());
                model.FriendInput.FriendId = newFr.Item.FriendId;
            }

            // Detect address changes even if status is Unchanged
            if (model.FriendInput.Address.StatusIM == StatusIM.Unchanged && model.FriendInput.Address.AddressId != null)
            {
                var existingFriend = await _friendsService.ReadFriendAsync(model.FriendInput.FriendId, false);
                if (existingFriend.Item.Address != null)
                {
                    if (existingFriend.Item.Address.StreetAddress != model.FriendInput.Address.StreetAddress ||
                        existingFriend.Item.Address.ZipCode != model.FriendInput.Address.ZipCode ||
                        existingFriend.Item.Address.City != model.FriendInput.Address.City ||
                        existingFriend.Item.Address.Country != model.FriendInput.Address.Country)
                    {
                        model.FriendInput.Address.StatusIM = StatusIM.Modified;
                    }
                }
            }
            // Create new address if fields are filled but no AddressId exists
            else if (model.FriendInput.Address.AddressId == null && 
                    (!string.IsNullOrEmpty(model.FriendInput.Address.StreetAddress) ||
                    !string.IsNullOrEmpty(model.FriendInput.Address.City) ||
                    !string.IsNullOrEmpty(model.FriendInput.Address.Country) ||
                    model.FriendInput.Address.ZipCode > 0))
            {
                model.FriendInput.Address.StatusIM = StatusIM.Inserted;
                model.FriendInput.Address.AddressId = Guid.NewGuid();
            }

            // Save all related entities
            var fr = await SaveAddress(model.FriendInput);
            fr = await SaveQuotes(model.FriendInput);
            fr = await SavePets(model.FriendInput);

            // Update the friend entity itself
            fr = model.FriendInput.UpdateModel(fr);
            await _friendsService.UpdateFriendAsync(new FriendCuDto(fr));

            // Redirect based on operation type
            if (model.FriendInput.StatusIM == StatusIM.Inserted)
            {
                return RedirectToAction("List");
            }

            return RedirectToAction("Details", new { id = model.FriendInput.FriendId });
        }

        [HttpGet]
        public async Task<IActionResult> Overview()
        {
            var info = await _adminService.GuestInfoAsync();

            var vm = new FriendsOverviewViewModel
            {
                CountryInfo = info.Item.Friends.Where(f => f.City == null && f.Country != null)
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> FriendsAndPetsOverview()
        {
            var friendsInfo = await _adminService.GuestInfoAsync();
            var petsInfo = await _adminService.GuestInfoAsync();

            var vm = new FriendsAndPetsOverviewViewModel
            {
                CountryInfo = friendsInfo.Item.Friends.Where(f => f.City == null && f.Country != null),
                PetsInfo = petsInfo.Item.Pets.Where(p => p.City == null && p.Country != null)
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> FriendsAndPetsCityOverview(string country)
        {
            var friendsInfo = await _adminService.GuestInfoAsync();
            var petsInfo = await _adminService.GuestInfoAsync();

            var vm = new FriendsAndPetsCityOverviewViewModel
            {
                CountryInfo = friendsInfo.Item.Friends.Where(f => f.City == null && f.Country == country),
                CityInfo = friendsInfo.Item.Friends.Where(f => f.City != null && f.Country == country),
                PetsInfo = petsInfo.Item.Pets.Where(p => p.City != null && p.Country == country)
            };

            return View(vm);
        }

        private void UpdatePagination(FriendsListViewModel vm, int nrOfItems)
        {
            vm.NrOfPages = (int)Math.Ceiling((double)nrOfItems / vm.PageSize);
            vm.PrevPageNr = Math.Max(0, vm.ThisPageNr - 1);
            vm.NextPageNr = Math.Min(vm.NrOfPages - 1, vm.ThisPageNr + 1);
            vm.NrVisiblePages = Math.Min(10, vm.NrOfPages);
        }

        #region Helpers
        private async Task<IFriend> SaveAddress(FriendIM friendInput)
        {
            var fr = await _friendsService.ReadFriendAsync(friendInput.FriendId, false);

            if (friendInput.Address.StatusIM == StatusIM.Deleted && friendInput.Address.AddressId != null)
            {
                await _addressesService.DeleteAddressAsync(friendInput.Address.AddressId.Value);

                var friendDto = new FriendCuDto(fr.Item);
                friendDto.AddressId = null;
                await _friendsService.UpdateFriendAsync(friendDto);
            }

            if (friendInput.Address.StatusIM == StatusIM.Inserted)
            {
                var newAddress = await _addressesService.CreateAddressAsync(friendInput.Address.CreateCuDto());
                friendInput.Address.AddressId = newAddress.Item.AddressId;

                var friendDto = new FriendCuDto(fr.Item);
                friendDto.AddressId = newAddress.Item.AddressId;
                await _friendsService.UpdateFriendAsync(friendDto);
            }

            fr = await _friendsService.ReadFriendAsync(friendInput.FriendId, false);

            if (friendInput.Address.StatusIM == StatusIM.Modified && friendInput.Address.AddressId != null)
            {
                var model = fr.Item.Address;
                model = friendInput.Address.UpdateModel(model);

                await _addressesService.UpdateAddressAsync(new AddressCuDto(model));
            }

            return fr.Item;
        }

        private async Task<IFriend> SaveQuotes(FriendIM friendInput)
        {
            var deletedQuotes = friendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Deleted));
            foreach (var item in deletedQuotes)
            {
                await _quotesService.DeleteQuoteAsync(item.QuoteId);
            }

            await _friendsService.ReadFriendAsync(friendInput.FriendId, false);

            var newQuotes = friendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Inserted));
            foreach (var item in newQuotes)
            {
                var cuDto = item.CreateCuDto();
                cuDto.FriendsId = [friendInput.FriendId];
                await _quotesService.CreateQuoteAsync(cuDto);
            }

            var fr = await _friendsService.ReadFriendAsync(friendInput.FriendId, false);

            var modifiedQuotes = friendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Modified));
            foreach (var item in modifiedQuotes)
            {
                var model = fr.Item.Quotes.First(q => q.QuoteId == item.QuoteId);
                model = item.UpdateModel(model);

                await _quotesService.UpdateQuoteAsync(new QuoteCuDto(model));
            }

            return fr.Item;
        }

        private async Task<IFriend> SavePets(FriendIM friendInput)
        {
            var deletedPets = friendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Deleted));
            foreach (var item in deletedPets)
            {
                await _petsService.DeletePetAsync(item.PetId);
            }

            await _friendsService.ReadFriendAsync(friendInput.FriendId, false);

            var newPets = friendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Inserted));
            foreach (var item in newPets)
            {
                var cuDto = item.CreateCuDto();
                cuDto.FriendId = friendInput.FriendId;
                await _petsService.CreatePetAsync(cuDto);
            }

            var fr = await _friendsService.ReadFriendAsync(friendInput.FriendId, false);

            var modifiedPets = friendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Modified));
            foreach (var item in modifiedPets)
            {
                var model = fr.Item.Pets.First(p => p.PetId == item.PetId);
                model = item.UpdateModel(model);

                await _petsService.UpdatePetAsync(new PetCuDto(model));
            }
            
            return fr.Item;
        }
        #endregion
    }
}