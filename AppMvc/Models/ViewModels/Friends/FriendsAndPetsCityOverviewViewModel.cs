using Models.DTO;

namespace AppMvc.Models.ViewModels.Friends
{
    public class FriendsAndPetsCityOverviewViewModel
    {
        public IEnumerable<GstUsrInfoFriendsDto>? CountryInfo { get; set; }
        public IEnumerable<GstUsrInfoFriendsDto>? CityInfo { get; set; }
        public IEnumerable<GstUsrInfoPetsDto>? PetsInfo { get; set; }
        public string? Country { get; set; }
    }
}
