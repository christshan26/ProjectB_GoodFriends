using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Interfaces;
using Models.DTO;
using Services.Interfaces;
using AppRazor.SeidoHelpers;
using Newtonsoft.Json;

namespace AppRazor.Pages.Friends.Detailview
{
    public class EditFriendModel : PageModel
    {
        private readonly IFriendsService _friendsService = null;
        private readonly IAddressesService _addressesService = null;
        private readonly IPetsService _petsService = null;
        private readonly IQuotesService _quotesService = null;
        public IFriend Friend { get; set; }
        public IAddress Address { get; set; }

        [BindProperty]
        public FriendIM FriendInput { get; set; }

        [BindProperty]
        public string PageHeader { get; set; }

        public List<SelectListItem> AnimalKind { get; set;} = new List<SelectListItem>().PopulateSelectList<AnimalKind>();

        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);

        #region HTTP Requests
        public async Task<IActionResult> OnGet()
        {
            if (Guid.TryParse(Request.Query["id"], out Guid _friendId))
            {
                var fr = await _friendsService.ReadFriendAsync(_friendId, false);

                FriendInput = new FriendIM(fr.Item);

                PageHeader = "Edit details for a friend";
            }
            else
            {
                FriendInput = new FriendIM();
                FriendInput.StatusIM = StatusIM.Inserted;

                PageHeader = "Create New Friend";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEditAddress()
        {
            string[] keys = { "FriendInput.Address.StreetAddress", "FriendInput.Address.ZipCode", "FriendInput.Address.City", "FriendInput.Address.Country" };

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            if (FriendInput.Address.AddressId == null || FriendInput.Address.AddressId == Guid.Empty)
            {
                FriendInput.Address.StatusIM = StatusIM.Inserted;
                FriendInput.Address.AddressId = Guid.NewGuid();
            }
            else if (FriendInput.Address.StatusIM != StatusIM.Inserted)
            {
                FriendInput.Address.StatusIM = StatusIM.Modified;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAddress()
        {
            if (FriendInput.Address.AddressId != null)
            {
                FriendInput.Address.StatusIM = StatusIM.Deleted;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteQuotes(Guid quoteId)
        {
            var quote = FriendInput.Quotes.FirstOrDefault(q => q.QuoteId == quoteId);
            if (quote != null)
            {
                quote.StatusIM = StatusIM.Deleted;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAddQuote()
        {
            string[] keys = { "FriendInput.NewQuote.QuoteText", "FriendInput.NewQuote.Author" };

            if(!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            FriendInput.NewQuote.StatusIM = StatusIM.Inserted;
            FriendInput.NewQuote.QuoteId = Guid.NewGuid();
            FriendInput.Quotes.Add(new QuoteIM(FriendInput.NewQuote));
            FriendInput.NewQuote = new QuoteIM();

            return Page();
        }

        public async Task<IActionResult> OnPostEditQuote(Guid quoteId)
        {
            int idx = FriendInput.Quotes.FindIndex(q => q.QuoteId == quoteId);
            string[] keys = { $"FriendInput.Quotes[{idx}].editQuoteText", $"FriendInput.Quotes[{idx}].editAuthor" };

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            var q = FriendInput.Quotes.First(q => q.QuoteId == quoteId);
            if (q.StatusIM != StatusIM.Inserted)
            {
                q.StatusIM = StatusIM.Modified;
            }

            q.QuoteText = q.editQuoteText;
            q.Author = q.editAuthor;

            return Page();
        }

        public async Task<IActionResult> OnPostDeletePets(Guid petId)
        {
            var pet = FriendInput.Pets.FirstOrDefault(p => p.PetId == petId);
            if (pet != null)
            {
                pet.StatusIM = StatusIM.Deleted;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAddPet()
        {
            string[] keys = { "FriendInput.NewPet.Name", "FriendInput.NewPet.Kind" };

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            FriendInput.NewPet.StatusIM = StatusIM.Inserted;
            FriendInput.NewPet.PetId = Guid.NewGuid();
            FriendInput.Pets.Add(new PetIM(FriendInput.NewPet));
            FriendInput.NewPet = new PetIM();

            return Page();
        }

        public async Task<IActionResult> OnPostEditPet(Guid petId)
        {
            int idx = FriendInput.Pets.FindIndex(p => p.PetId == petId);
            string[] keys = { $"FriendInput.Pets[{idx}].editName", $"FriendInput.Pets[{idx}].editKind" };

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            var p = FriendInput.Pets.First(p => p.PetId == petId);
            if (p.StatusIM != StatusIM.Inserted)
            {
                p.StatusIM = StatusIM.Modified;
            }

            p.Name = p.editName;
            p.Kind = p.editKind;

            return Page();
        }

        public async Task<IActionResult> OnPostUndo()
        {
            var fr = await _friendsService.ReadFriendAsync(FriendInput.FriendId, false);
            FriendInput = new FriendIM(fr.Item);
            return Page();
        }

        /* Optional Undo that reverts all changes by reloading the page
        public async Task<IActionResult> OnPostUndo()
        {
            return RedirectToPage(new { id = FriendInput.FriendId });
        }
        */

        public async Task<IActionResult> OnPostSave()
        {
            string[] keys = { "FriendInput.FirstName", "FriendInput.LastName" };

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            if (FriendInput.StatusIM == StatusIM.Inserted)
            {
                var newFr = await _friendsService.CreateFriendAsync(FriendInput.CreateCUdto());
                FriendInput.FriendId = newFr.Item.FriendId;
            }

            if (FriendInput.Address.StatusIM == StatusIM.Unchanged && FriendInput.Address.AddressId != null)
            {
                var existingFriend = await _friendsService.ReadFriendAsync(FriendInput.FriendId, false);
                if (existingFriend.Item.Address != null)
                {
                    if (existingFriend.Item.Address.StreetAddress != FriendInput.Address.StreetAddress ||
                        existingFriend.Item.Address.ZipCode != FriendInput.Address.ZipCode ||
                        existingFriend.Item.Address.City != FriendInput.Address.City ||
                        existingFriend.Item.Address.Country != FriendInput.Address.Country)
                    {
                        FriendInput.Address.StatusIM = StatusIM.Modified;
                    }
                }
            }
            else if (FriendInput.Address.AddressId == null && 
                    (!string.IsNullOrEmpty(FriendInput.Address.StreetAddress) ||
                    !string.IsNullOrEmpty(FriendInput.Address.City) ||
                    !string.IsNullOrEmpty(FriendInput.Address.Country) ||
                    FriendInput.Address.ZipCode > 0))
            {
                FriendInput.Address.StatusIM = StatusIM.Inserted;
                FriendInput.Address.AddressId = Guid.NewGuid();
            }

            var fr = await SaveAddress();
            fr = await SaveQuotes();
            fr = await SavePets();

            fr = FriendInput.UpdateModel(fr);
            await _friendsService.UpdateFriendAsync(new FriendCuDto(fr));

            if(FriendInput.StatusIM == StatusIM.Inserted)
            {
                return Redirect($"/Friends/Lists/FriendsList");
            }

            return Redirect($"/Friends/Detailview/ViewFriend?id={FriendInput.FriendId}");
        }
        #endregion

        #region InputModel Quotes and Pets saved to database
        private async Task<IFriend> SaveAddress()
        {
            var fr = await _friendsService.ReadFriendAsync(FriendInput.FriendId, false);

            if (FriendInput.Address.StatusIM == StatusIM.Deleted && FriendInput.Address.AddressId != null)
            {
                await _addressesService.DeleteAddressAsync(FriendInput.Address.AddressId.Value);

                var friendDto = new FriendCuDto(fr.Item);
                friendDto.AddressId = null;
                await _friendsService.UpdateFriendAsync(friendDto);
            }

            if (FriendInput.Address.StatusIM == StatusIM.Inserted)
            {
                var newAddress = await _addressesService.CreateAddressAsync(FriendInput.Address.CreateCUdto());
                FriendInput.Address.AddressId = newAddress.Item.AddressId;

                var friendDto = new FriendCuDto(fr.Item);
                friendDto.AddressId = newAddress.Item.AddressId;
                await _friendsService.UpdateFriendAsync(friendDto);
            }

            fr = await _friendsService.ReadFriendAsync(FriendInput.FriendId, false);

            if (FriendInput.Address.StatusIM == StatusIM.Modified && FriendInput.Address.AddressId != null)
            {
                var model = fr.Item.Address;
                model = FriendInput.Address.UpdateModel(model);

                await _addressesService.UpdateAddressAsync(new AddressCuDto(model));
            }

            return fr.Item;
        }

        private async Task<IFriend> SaveQuotes()
        {
            var deletedQuotes = FriendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Deleted));
            foreach (var item in deletedQuotes)
            {
                await _quotesService.DeleteQuoteAsync(item.QuoteId);
            }

            await _friendsService.ReadFriendAsync(FriendInput.FriendId, false);

            var newQuotes = FriendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Inserted));
            foreach (var item in newQuotes)
            {
                var cuDto = item.CreateCUdto();
                cuDto.FriendsId = [FriendInput.FriendId];
                await _quotesService.CreateQuoteAsync(cuDto);
            }

            var fr = await _friendsService.ReadFriendAsync(FriendInput.FriendId, false);

            var modifiedQuotes = FriendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Modified));
            foreach (var item in modifiedQuotes)
            {
                var model = fr.Item.Quotes.First(q => q.QuoteId == item.QuoteId);
                model = item.UpdateModel(model);

                await _quotesService.UpdateQuoteAsync(new QuoteCuDto(model));
            }

            return fr.Item;
        }

        private async Task<IFriend> SavePets()
        {
            var deletedPets = FriendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Deleted));
            foreach (var item in deletedPets)
            {
                await _petsService.DeletePetAsync(item.PetId);
            }

            await _friendsService.ReadFriendAsync(FriendInput.FriendId, false);

            var newPets = FriendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Inserted));
            foreach (var item in newPets)
            {
                var cuDto = item.CreateCUdto();
                cuDto.FriendId = FriendInput.FriendId;
                await _petsService.CreatePetAsync(cuDto);
            }

            var fr = await _friendsService.ReadFriendAsync(FriendInput.FriendId, false);

            var modifiedPets = FriendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Modified));
            foreach (var item in modifiedPets)
            {
                var model = fr.Item.Pets.First(p => p.PetId == item.PetId);
                model = item.UpdateModel(model);

                await _petsService.UpdatePetAsync(new PetCuDto(model));
            }
            
            return fr.Item;
        }
        #endregion

        #region Constructors
        public EditFriendModel(IFriendsService f_service, IAddressesService a_service, IPetsService p_service, IQuotesService q_service)
        {
            _friendsService = f_service;
            _addressesService = a_service;
            _petsService = p_service;
            _quotesService = q_service;
        }
        #endregion

        #region Input Models
        public enum StatusIM { Unknown, Unchanged, Inserted, Modified, Deleted }

        public class PetIM
        {
            public StatusIM StatusIM { get; set; }
            public Guid PetId { get; set; }

            [Required(ErrorMessage = "Pet name is required")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Animal kind is required")]
            public AnimalKind? Kind { get; set; }

            [Required(ErrorMessage = "Pet name is required")]
            public string editName { get; set; }

            [Required(ErrorMessage = "Animal kind is required")]
            public AnimalKind? editKind { get; set; }

            public PetIM() { }

            public PetIM(PetIM original)
            {
                StatusIM = original.StatusIM;
                PetId = original.PetId;
                Name = original.Name;
                Kind = original.Kind;
                editName = original.editName;
                editKind = original.editKind;
            }

            public PetIM(IPet model)
            {
                StatusIM = StatusIM.Unchanged;
                PetId = model.PetId;
                Name = editName = model.Name;
                Kind = editKind = model.Kind;
            }

            public IPet UpdateModel(IPet model)
            {
                model.PetId = this.PetId;
                model.Name = this.Name;
                model.Kind = this.Kind.Value;
                return model;
            }

            public PetCuDto CreateCUdto() => new PetCuDto()
            {
                PetId = null,
                Name = this.Name,
                Kind = this.Kind.Value,
            };
        }

        public class QuoteIM
        {
            public StatusIM StatusIM { get; set; }
            public Guid QuoteId { get; set; }

            [Required(ErrorMessage = "Quote text is required")]
            public string QuoteText { get; set; }

            [Required(ErrorMessage = "Author is required")]
            public string Author { get; set; }

            [Required(ErrorMessage = "Quote text is required")]
            public string editQuoteText { get; set; }

            [Required(ErrorMessage = "Author is required")]
            public string editAuthor { get; set; }

            public QuoteIM() { }

            public QuoteIM(QuoteIM original)
            {
                StatusIM = original.StatusIM;
                QuoteId = original.QuoteId;
                QuoteText = original.QuoteText;
                Author = original.Author;
                editQuoteText = original.editQuoteText;
                editAuthor = original.editAuthor;
            }

            public QuoteIM(IQuote model)
            {
                StatusIM = StatusIM.Unchanged;
                QuoteId = model.QuoteId;
                QuoteText = editQuoteText = model.QuoteText;
                Author = editAuthor = model.Author;
            }

            public IQuote UpdateModel(IQuote model)
            {
                model.QuoteId = this.QuoteId;
                model.QuoteText = this.QuoteText;
                model.Author = this.Author;
                return model;
            }

            public QuoteCuDto CreateCUdto() => new QuoteCuDto()
            {
                QuoteId = null,
                Quote = this.QuoteText,
                Author = this.Author
            };
        }

        public class AddressIM
        {
            public StatusIM StatusIM { get; set; }
            public Guid? AddressId { get; set; }

            [Required(ErrorMessage = "Street is required")]
            public string? StreetAddress { get; set; }

            [Required(ErrorMessage = "Zip Code is required")]
            [Range(10101, 100000,ErrorMessage = "Zip Code between 10101 and 100000 is required")]
            public int? ZipCode { get; set; }

            [Required(ErrorMessage = "City is required")]
            public string? City { get; set; }

            [Required(ErrorMessage = "Country is required")]
            public string? Country { get; set; }

            public AddressIM() { }

            public AddressIM(IAddress model)
            {
                StatusIM = StatusIM.Unchanged;
                AddressId = model.AddressId;
                StreetAddress = model.StreetAddress;
                ZipCode = model.ZipCode;
                City = model.City;
                Country = model.Country;
            }

            public IAddress UpdateModel(IAddress model)
            {
                model.AddressId = this.AddressId ?? model.AddressId;
                model.StreetAddress = this.StreetAddress;
                model.ZipCode = this.ZipCode ?? model.ZipCode;
                model.City = this.City;
                model.Country = this.Country;
                return model;
            }

            public AddressCuDto CreateCUdto() => new AddressCuDto()
            {
                AddressId = null,
                StreetAddress = this.StreetAddress,
                ZipCode = this.ZipCode ?? 0,
                City = this.City,
                Country = this.Country
            };
        }

        public class FriendIM
        {
            public StatusIM StatusIM { get; set; }
            public Guid FriendId { get; set; }

            [BindProperty]
            public bool IsAddingAddress { get; set; }

            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Animal kind is required")]
            public AnimalKind AnimalKind { get; set; }

            public AddressIM Address { get; set; }
            public List<PetIM> Pets { get; set; } = new List<PetIM>();
            public List<QuoteIM> Quotes { get; set; } = new List<QuoteIM>();

            public FriendIM()
            {
                Address = new AddressIM();
            }
        
            public FriendIM(IFriend model)
            {
                StatusIM = StatusIM.Unchanged;
                FriendId = model.FriendId;
                FirstName = model.FirstName;
                LastName = model.LastName;

                Address = model.Address != null ? new AddressIM(model.Address) : new AddressIM();
                Pets = model.Pets?.Select(p => new PetIM(p)).ToList();
                Quotes = model.Quotes?.Select(q => new QuoteIM(q)).ToList();
            }

            public IFriend UpdateModel(IFriend model)
            {
                model.FriendId = this.FriendId;
                model.FirstName = this.FirstName;
                model.LastName = this.LastName;
                return model;
            }

            public FriendCuDto CreateCUdto() => new FriendCuDto()
            {
                FriendId = null,
                FirstName = this.FirstName,
                LastName = this.LastName
            };

            public PetIM NewPet { get; set; } = new PetIM();
            public QuoteIM NewQuote { get; set; } = new QuoteIM();
            public AddressIM NewAddress { get; set; } = new AddressIM();
        }
        #endregion
    }
}
