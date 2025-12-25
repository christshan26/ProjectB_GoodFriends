using Services.Interfaces;

namespace AppMvc.Models.ViewModels.Home
{
    public class IndexViewModel
    {
        private readonly ILogger<IndexViewModel> _logger;
        private readonly IAdminService _adminService;
        public int AddressCount { get; set; }
        public int FriendCount { get; set; }
        public int PetCount { get; set; }
        public int QuoteCount { get; set; }

        public IndexViewModel( ILogger<IndexViewModel> logger, IAdminService adminService)
        {
            _logger = logger;
            _adminService = adminService;
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var info = await _adminService.GuestInfoAsync();
                AddressCount = info.Item.Db.NrSeededAddresses + info.Item.Db.NrUnseededAddresses;
                FriendCount = info.Item.Db.NrSeededFriends + info.Item.Db.NrUnseededFriends;
                PetCount = info.Item.Db.NrSeededPets + info.Item.Db.NrUnseededPets;
                QuoteCount = info.Item.Db.NrSeededQuotes + info.Item.Db.NrUnseededQuotes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in guest info retrieval");
                throw;
            }
        }
    }
}