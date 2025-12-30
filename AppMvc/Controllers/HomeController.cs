using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AppMvc.Models;
using Services.Interfaces;
using AppMvc.Models.ViewModels.Home;

namespace AppMvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAdminService _adminService;

    public HomeController(ILogger<HomeController> logger, IAdminService adminService)
    {
        _logger = logger;
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new IndexViewModel();

        try
        {
            var info =  await _adminService.GuestInfoAsync();
            vm.AddressCount = info.Item.Db.NrSeededAddresses + info.Item.Db.NrUnseededAddresses;
            vm.FriendCount = info.Item.Db.NrSeededFriends + info.Item.Db.NrUnseededFriends;
            vm.PetCount = info.Item.Db.NrSeededPets + info.Item.Db.NrUnseededPets;
            vm.QuoteCount = info.Item.Db.NrSeededQuotes + info.Item.Db.NrUnseededQuotes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in guest info retrieval");
            return StatusCode(500, "Internal server error");
        }

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
