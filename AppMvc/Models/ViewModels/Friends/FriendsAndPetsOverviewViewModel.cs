using Models.DTO;

namespace AppMvc.Models.ViewModels.Friends
{
    public class FriendsAndPetsOverviewViewModel
    {
        public IEnumerable<GstUsrInfoFriendsDto>? CountryInfo { get; set; }
        public IEnumerable<GstUsrInfoPetsDto>? PetsInfo { get; set; }
    }
}
