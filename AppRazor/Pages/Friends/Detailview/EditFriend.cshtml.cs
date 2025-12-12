using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Interfaces;
using Models.DTO;
using Services;
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

        public List<SelectListItem> AnimalKinds { get; set;} = new List<SelectListItem>().PopulateSelectList<AnimalKind>();

        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);

        public async Task<IActionResult> OnGet()
        {
            Guid _friendId = Guid.Parse(Request.Query["id"]);
            Friend = (await _friendsService.ReadFriendAsync(_friendId, false)).Item;

            return Page();
        }

        #region InputModel Quotes and Pets saved to database
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
            return null;
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

            //public List<FriendIM> Friends { get; set; } = new List<FriendIM>();

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

                //Friends = model.Friends?.Select(f => new FriendIM(f)).ToList();
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

            //public FriendIM NewFriend { get; set; } = new FriendIM();
        }

        public class AddressIM
        {
            public StatusIM StatusIM { get; set; }
            public Guid AddressId { get; set; }

            [Required(ErrorMessage = "Street is required")]
            public string StreetAddress { get; set; }

            [Range(10101, 100000,ErrorMessage = "Zip Code between 10101 and 100000 is required")]
            public int ZipCode { get; set; }

            [Required(ErrorMessage = "City is required")]
            public string City { get; set; }

            [Required(ErrorMessage = "Country is required")]
            public string Country { get; set; }

            public List<FriendIM> Friends { get; set; } = new List<FriendIM>();

            public AddressIM() { }

            /*public AddressIM(AddressIM original)
            {
                StatusIM = original.StatusIM;
                AddressId = original.AddressId;
                StreetAddress = original.StreetAddress;
                ZipCode = original.ZipCode;
                City = original.City;
                Country = original.Country;
            }*/

            public AddressIM(IAddress model)
            {
                StatusIM = StatusIM.Unchanged;
                AddressId = model.AddressId;
                StreetAddress = model.StreetAddress;
                ZipCode = model.ZipCode;
                City = model.City;
                Country = model.Country;

                Friends = model.Friends?.Select(f => new FriendIM(f)).ToList();
            }

            public IAddress UpdateModel(IAddress model)
            {
                model.AddressId = this.AddressId;
                model.StreetAddress = this.StreetAddress;
                model.ZipCode = this.ZipCode;
                model.City = this.City;
                model.Country = this.Country;
                return model;
            }

            public AddressCuDto CreateCUdto() => new AddressCuDto()
            {
                AddressId = null,
                StreetAddress = this.StreetAddress,
                ZipCode = this.ZipCode,
                City = this.City,
                Country = this.Country
            };

            public FriendIM NewFriend { get; set; } = new FriendIM();
        }

        public class FriendIM
        {
            public StatusIM StatusIM { get; set; }
            public Guid FriendId { get; set; }

            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            public List<PetIM> Pets { get; set; } = new List<PetIM>();
            public List<QuoteIM> Quotes { get; set; } = new List<QuoteIM>();

            public FriendIM() {}

            /*public FriendIM(FriendIM original)
            {
                StatusIM = original.StatusIM;
                FriendId = original.FriendId;
                FirstName = original.FirstName;
                LastName = original.LastName;
            }*/

            public FriendIM(IFriend model)
            {
                StatusIM = StatusIM.Unchanged;
                FriendId = model.FriendId;
                FirstName = model.FirstName;
                LastName = model.LastName;

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
        }
        #endregion
    }
}
