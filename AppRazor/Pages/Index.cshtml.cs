using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services.Interfaces;


namespace AppRazor.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IAdminService _adminService;

    public int AddressCount { get; set; }
    public int FriendCount { get; set; }
    public int PetCount { get; set; }
    public int QuoteCount { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IAdminService adminService)
    {
        _logger = logger;
        _adminService = adminService;
    }

    public async Task<IActionResult> OnGet()
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
            return StatusCode(500, "Internal server error");
        }
        return Page();
    }
}
