using Models.Interfaces;
using Models.DTO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AppMvc.SeidoHelpers;
using System.ComponentModel.DataAnnotations;


namespace AppMvc.Models.ViewModels.Friends
{
    public class EditFriendViewModel
    {
        private readonly IFriendsService _friendsService;
        private readonly IAddressesService _addressesService;
        private readonly IPetsService _petsService;
        private readonly IQuotesService _quotesService;
        public IFriend Friend { get; set; }
        public IAddress Address { get; set; }

        [BindProperty]
        public FriendIM FriendInput { get; set; }

        [BindProperty]
        public string PageHeader { get; set; }

        public List<SelectListItem> AnimalKind { get; set; } = new List<SelectListItem>().PopulateSelectList<AnimalKind>();
        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);

        #region Inputmodels
        public enum StatusIM { Unknown, Unchanged, Inserted, Modified, Deleted}

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

            public PetIM()
            {
            }

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
                Name = model.Name;
                Kind = model.Kind;
            }

            public IPet UpdateModel(IPet model)
            {
                model.PetId = this.PetId;
                model.Name = this.Name;
                model.Kind = this.Kind.Value;
                return model;
            }

            public PetCuDto CreateCuDto() => new PetCuDto()
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

            public QuoteIM()
            {
            }

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
                return model;
            }

            public QuoteCuDto CreateCuDto() => new QuoteCuDto()
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

            [Required(ErrorMessage = "City is required")]
            public string? City { get; set; }

            [Required(ErrorMessage = "Country is required")]
            public string? Country { get; set; }

            [Required(ErrorMessage = "Zip code is required")]
            [Range(10101, 100000,ErrorMessage = "Zip Code between 10101 and 100000 is required")]
            public int? ZipCode { get; set; }
            public AddressIM()
            {
            }

            public AddressIM(IAddress model)
            {
                StatusIM = StatusIM.Unchanged;
                AddressId = model.AddressId;
                StreetAddress = model.StreetAddress;
                City = model.City;
                Country = model.Country;
                ZipCode = model.ZipCode;
            }

            public IAddress UpdateModel(IAddress model)
            {
                model.AddressId = this.AddressId ?? model.AddressId;
                model.StreetAddress = this.StreetAddress;
                model.City = this.City;
                model.Country = this.Country;
                model.ZipCode = this.ZipCode ?? model.ZipCode;
                return model;
            }

            public AddressCuDto CreateCuDto() => new AddressCuDto()
            {
                AddressId = null,
                StreetAddress = this.StreetAddress,
                City = this.City,
                Country = this.Country,
                ZipCode = this.ZipCode ?? 0
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
                model.FriendId = FriendId;
                model.FirstName = FirstName;
                model.LastName = LastName;
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

        public EditFriendViewModel()
        {
            AnimalKind = new List<SelectListItem>();
        }
    }
}