# Comprehensive Guide: Refactoring AppRazor to AppMvc

## Overview
This guide provides a step-by-step approach to migrate a Razor Pages application (AppRazor) to an MVC application (AppMvc). The migration involves converting Razor Pages with PageModels to Controllers with Actions, Views, and ViewModels following the MVC architectural pattern.

## Table of Contents
1. [Understanding the Differences](#understanding-the-differences)
2. [Pre-Migration Checklist](#pre-migration-checklist)
3. [Migration Strategy](#migration-strategy)
4. [Step-by-Step Migration Process](#step-by-step-migration-process)
5. [Common Patterns and Conversions](#common-patterns-and-conversions)
6. [Testing and Validation](#testing-and-validation)

---

## Understanding the Differences

### Razor Pages vs MVC

| Aspect | Razor Pages (AppRazor) | MVC (AppMvc) |
|--------|----------------------|--------------|
| **Structure** | Page-centric (PageModel + .cshtml) | Controller-centric (Controller → View) |
| **File Organization** | Pages/Feature/PageName.cshtml + .cs | Controllers/FeatureController.cs + Views/Feature/ActionName.cshtml |
| **Routing** | Convention-based from folder structure | Attribute or convention-based on controller/action |
| **Handler Methods** | OnGet(), OnPost(), OnPostActionName() | ActionResult methods (Index(), Edit(), etc.) |
| **Model Binding** | [BindProperty] on PageModel | Method parameters or [BindProperty] on controller |
| **Return Types** | Page(), RedirectToPage() | View(), RedirectToAction() |

---

## Pre-Migration Checklist

### 1. Analyze Current AppRazor Structure
```
AppRazor/Pages/
├── Index.cshtml + Index.cshtml.cs
├── Privacy.cshtml + Privacy.cshtml.cs
├── Error.cshtml + Error.cshtml.cs
├── Admin/
│   └── Seed.cshtml + Seed.cshtml.cs
└── Friends/
    ├── Lists/
    │   └── FriendsList.cshtml + FriendsList.cshtml.cs
    ├── Detailview/
    │   ├── ViewFriend.cshtml + ViewFriend.cshtml.cs
    │   └── EditFriend.cshtml + EditFriend.cshtml.cs
    └── Overviews/
        ├── FriendsOverview.cshtml + FriendsOverview.cshtml.cs
        ├── FriendsAndPetsOverview.cshtml + FriendsAndPetsOverview.cshtml.cs
        └── FriendsAndPetsCityOverview.cshtml + FriendsAndPetsCityOverview.cshtml.cs
```

### 2. Identify Required Components
- [ ] PageModels to convert → Controllers
- [ ] Pages to convert → Views
- [ ] Data Transfer Objects → ViewModels
- [ ] Services and dependencies
- [ ] Routing patterns
- [ ] Form handlers (POST methods)
- [ ] Validation logic

### 3. Map Pages to MVC Structure
```
Razor Page → MVC Equivalent
-----------------------------
Pages/Index → Home/Index
Pages/Admin/Seed → Admin/Seed
Pages/Friends/Lists/FriendsList → Friends/List
Pages/Friends/Detailview/ViewFriend → Friends/Details
Pages/Friends/Detailview/EditFriend → Friends/Edit (GET + POST)
Pages/Friends/Overviews/FriendsOverview → Friends/Overview
```

---

## Migration Strategy

### Phase 1: Setup Foundation
1. Ensure Program.cs is configured for MVC
2. Create necessary ViewModels
3. Set up shared layouts and partials

### Phase 2: Controller Creation
1. Create controllers for each feature area
2. Migrate handler methods to controller actions
3. Inject required services

### Phase 3: View Migration
1. Convert .cshtml pages to views
2. Update model directives
3. Update form handlers and URLs
4. Update navigation links

### Phase 4: Testing & Refinement
1. Test each migrated page
2. Validate routing
3. Verify form submissions
4. Check validation

---

## Step-by-Step Migration Process

### STEP 1: Verify Program.cs Configuration

**Current AppMvc Program.cs:**
```csharp
// ✓ Already configured
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
```

**Routing Configuration:**
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

✅ **Status:** AppMvc is already configured correctly.

---

### STEP 2: Create ViewModels for Each Feature

Create a `ViewModels` folder structure in AppMvc:

```
AppMvc/
└── Models/
    └── ViewModels/
        ├── Home/
        │   └── IndexViewModel.cs
        ├── Admin/
        │   └── SeedViewModel.cs
        └── Friends/
            ├── FriendsListViewModel.cs
            ├── FriendDetailsViewModel.cs
            ├── EditFriendViewModel.cs
            └── FriendsOverviewViewModel.cs
```

#### Example ViewModel Creation

**FriendsListViewModel.cs:**
```csharp
using Models.Interfaces;

namespace AppMvc.Models.ViewModels.Friends
{
    public class FriendsListViewModel
    {
        // Data properties
        public List<IFriend> FriendsList { get; set; }
        public int NrOfFriends { get; set; }
        
        // Pagination
        public int NrOfPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int ThisPageNr { get; set; } = 0;
        public int PrevPageNr { get; set; } = 0;
        public int NextPageNr { get; set; } = 0;
        public int NrVisiblePages { get; set; } = 0;
        
        // Filters
        public bool UseSeeds { get; set; } = true;
        public string SearchFilter { get; set; }
    }
}
```

**EditFriendViewModel.cs:**
```csharp
using Models.Interfaces;
using Models.DTO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppMvc.Models.ViewModels.Friends
{
    public class EditFriendViewModel
    {
        public IFriend Friend { get; set; }
        public IAddress Address { get; set; }
        public FriendIM FriendInput { get; set; }
        public string PageHeader { get; set; }
        public List<SelectListItem> AnimalKind { get; set; }
        public ModelValidationResult ValidationResult { get; set; }
    }
}
```

**IndexViewModel.cs (Home):**
```csharp
namespace AppMvc.Models.ViewModels.Home
{
    public class IndexViewModel
    {
        public int AddressCount { get; set; }
        public int FriendCount { get; set; }
        public int PetCount { get; set; }
        public int QuoteCount { get; set; }
    }
}
```

---

### STEP 3: Create Controllers

#### 3.1 Migrate Home/Index Page

**Source:** `AppRazor/Pages/Index.cshtml.cs`

**Target:** `AppMvc/Controllers/HomeController.cs` (already exists, add to it)

**Add Index Action:**
```csharp
using AppMvc.Models.ViewModels.Home;

public async Task<IActionResult> Index()
{
    var vm = new IndexViewModel();
    
    try
    {
        var info = await _adminService.GuestInfoAsync();
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
```

#### 3.2 Create FriendsController

**Create:** `AppMvc/Controllers/FriendsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppMvc.Models.ViewModels.Friends;
using Services.Interfaces;
using Models.DTO;
using AppMvc.SeidoHelpers; // If using helper extensions

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
            IQuotesService quotesService)
        {
            _logger = logger;
            _friendsService = friendsService;
            _addressesService = addressesService;
            _petsService = petsService;
            _quotesService = quotesService;
        }

        // GET: Friends/List
        [HttpGet]
        public async Task<IActionResult> List(int pagenr = 0, string search = null, bool useSeeds = true)
        {
            var vm = new FriendsListViewModel
            {
                ThisPageNr = pagenr,
                SearchFilter = search,
                UseSeeds = useSeeds
            };

            var resp = await _friendsService.ReadFriendsAsync(useSeeds, false, search, pagenr, vm.PageSize);
            vm.FriendsList = resp.PageItems;
            vm.NrOfFriends = resp.DbItemsCount;

            UpdatePagination(vm, resp.DbItemsCount);

            return View(vm);
        }

        // POST: Friends/Search
        [HttpPost]
        public async Task<IActionResult> Search(FriendsListViewModel model)
        {
            var resp = await _friendsService.ReadFriendsAsync(
                model.UseSeeds, false, model.SearchFilter, model.ThisPageNr, model.PageSize);
            
            model.FriendsList = resp.PageItems;
            model.NrOfFriends = resp.DbItemsCount;

            UpdatePagination(model, resp.DbItemsCount);

            return View("List", model);
        }

        // POST: Friends/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id, FriendsListViewModel model)
        {
            await _friendsService.DeleteFriendAsync(id);

            var resp = await _friendsService.ReadFriendsAsync(
                model.UseSeeds, false, model.SearchFilter, model.ThisPageNr, model.PageSize);
            
            model.FriendsList = resp.PageItems;
            model.NrOfFriends = resp.DbItemsCount;

            UpdatePagination(model, resp.DbItemsCount);

            return View("List", model);
        }

        // GET: Friends/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var friend = await _friendsService.ReadFriendAsync(id, true);
            
            var vm = new FriendDetailsViewModel
            {
                Friend = friend.Item
            };

            return View(vm);
        }

        // GET: Friends/Edit/{id?}
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
                vm.PageHeader = "Create New Friend";
            }

            return View(vm);
        }

        // POST: Friends/EditAddress
        [HttpPost]
        public async Task<IActionResult> EditAddress(EditFriendViewModel model)
        {
            string[] keys = { 
                "FriendInput.Address.StreetAddress", 
                "FriendInput.Address.ZipCode", 
                "FriendInput.Address.City", 
                "FriendInput.Address.Country" 
            };

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

        // POST: Friends/Save
        [HttpPost]
        public async Task<IActionResult> Save(EditFriendViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ValidationResult = new ModelValidationResult(false, ModelState, null);
                return View("Edit", model);
            }

            // Save logic here
            var resp = await _friendsService.UpdateFriendAsync(model.FriendInput);

            if (resp.OperationSuccess)
            {
                return RedirectToAction("List");
            }

            return View("Edit", model);
        }

        // GET: Friends/Overview
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

        private void UpdatePagination(FriendsListViewModel vm, int nrOfItems)
        {
            vm.NrOfPages = (int)Math.Ceiling((double)nrOfItems / vm.PageSize);
            vm.PrevPageNr = Math.Max(0, vm.ThisPageNr - 1);
            vm.NextPageNr = Math.Min(vm.NrOfPages - 1, vm.ThisPageNr + 1);
            vm.NrVisiblePages = Math.Min(10, vm.NrOfPages);
        }
    }
}
```

#### 3.3 Create AdminController

**Create:** `AppMvc/Controllers/AdminController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using AppMvc.Models.ViewModels.Admin;
using Services.Interfaces;

namespace AppMvc.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IAdminService _adminService;

        public AdminController(ILogger<AdminController> logger, IAdminService adminService)
        {
            _logger = logger;
            _adminService = adminService;
        }

        // GET: Admin/Seed
        [HttpGet]
        public IActionResult Seed()
        {
            var vm = new SeedViewModel();
            return View(vm);
        }

        // POST: Admin/Seed
        [HttpPost]
        public async Task<IActionResult> Seed(SeedViewModel model)
        {
            try
            {
                await _adminService.SeedAsync(model.NrOfItems, model.IncludeRelations);
                model.Success = true;
                model.Message = $"Successfully seeded {model.NrOfItems} items.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding database");
                model.Success = false;
                model.Message = "Error seeding database: " + ex.Message;
            }

            return View(model);
        }
    }
}
```

---

### STEP 4: Convert Views

#### 4.1 Update Model Directive

**Razor Pages Pattern:**
```cshtml
@page
@model AppRazor.Pages.Friends.Lists.FriendsListModel
```

**MVC Pattern:**
```cshtml
@model AppMvc.Models.ViewModels.Friends.FriendsListViewModel
```

#### 4.2 Update Form Handlers

**Razor Pages Pattern:**
```cshtml
<form method="post" asp-page-handler="Search">
    <!-- form content -->
</form>
```

**MVC Pattern:**
```cshtml
<form method="post" asp-controller="Friends" asp-action="Search">
    <!-- form content -->
</form>
```

#### 4.3 Update Navigation Links

**Razor Pages Pattern:**
```cshtml
<a asp-page="/Friends/Lists/FriendsList">View Friends</a>
<a asp-page="/Friends/Detailview/EditFriend" asp-route-id="@friend.FriendId">Edit</a>
```

**MVC Pattern:**
```cshtml
<a asp-controller="Friends" asp-action="List">View Friends</a>
<a asp-controller="Friends" asp-action="Edit" asp-route-id="@friend.FriendId">Edit</a>
```

#### 4.4 Example View Migration: FriendsList

**Source:** `AppRazor/Pages/Friends/Lists/FriendsList.cshtml`

**Target:** `AppMvc/Views/Friends/List.cshtml`

**Create the view directory structure:**
```
AppMvc/Views/
└── Friends/
    ├── List.cshtml
    ├── Details.cshtml
    ├── Edit.cshtml
    └── Overview.cshtml
```

**Migration Steps:**
1. Remove `@page` directive
2. Update `@model` directive
3. Update all form handlers
4. Update all navigation links
5. Update any RedirectToPage calls in JavaScript/client-side code

**Example before:**
```cshtml
@page
@model AppRazor.Pages.Friends.Lists.FriendsListModel

<form method="post" asp-page-handler="DeleteFriend" asp-route-id="@friend.FriendId">
    <button type="submit">Delete</button>
</form>

<a asp-page="/Friends/Detailview/EditFriend" asp-route-id="@friend.FriendId">Edit</a>
```

**Example after:**
```cshtml
@model AppMvc.Models.ViewModels.Friends.FriendsListViewModel

<form method="post" asp-controller="Friends" asp-action="Delete" asp-route-id="@friend.FriendId">
    <button type="submit">Delete</button>
</form>

<a asp-controller="Friends" asp-action="Edit" asp-route-id="@friend.FriendId">Edit</a>
```

---

### STEP 5: Handle Complex Forms (EditFriend Example)

The EditFriend page has multiple POST handlers. In MVC, these become separate actions.

**Razor Pages Handlers:**
- `OnGet()` → GET action
- `OnPostEditAddress()` → POST action
- `OnPostDeleteAddress()` → POST action
- `OnPostAddPet()` → POST action
- `OnPostEditPet()` → POST action
- `OnPostDeletePet()` → POST action
- `OnPostSave()` → POST action

**MVC Actions:**
```csharp
[HttpGet]
public async Task<IActionResult> Edit(Guid? id) { }

[HttpPost]
public async Task<IActionResult> EditAddress(EditFriendViewModel model) { }

[HttpPost]
public async Task<IActionResult> DeleteAddress(EditFriendViewModel model) { }

[HttpPost]
public async Task<IActionResult> AddPet(EditFriendViewModel model) { }

[HttpPost]
public async Task<IActionResult> EditPet(EditFriendViewModel model) { }

[HttpPost]
public async Task<IActionResult> DeletePet(Guid petId, EditFriendViewModel model) { }

[HttpPost]
public async Task<IActionResult> Save(EditFriendViewModel model) { }
```

**View Form Updates:**
```cshtml
<!-- Razor Pages -->
<form method="post" asp-page-handler="EditAddress">

<!-- MVC -->
<form method="post" asp-controller="Friends" asp-action="EditAddress">
```

---

### STEP 6: Update Shared Components

#### 6.1 Layout Files

Update `_Layout.cshtml` navigation links:

**Before:**
```cshtml
<a class="nav-link" asp-page="/Index">Home</a>
<a class="nav-link" asp-page="/Friends/Lists/FriendsList">Friends</a>
<a class="nav-link" asp-page="/Admin/Seed">Admin</a>
```

**After:**
```cshtml
<a class="nav-link" asp-controller="Home" asp-action="Index">Home</a>
<a class="nav-link" asp-controller="Friends" asp-action="List">Friends</a>
<a class="nav-link" asp-controller="Admin" asp-action="Seed">Admin</a>
```

#### 6.2 _ViewImports.cshtml

Update namespace references:

```cshtml
@using AppMvc
@using AppMvc.Models
@using AppMvc.Models.ViewModels
@using AppMvc.Models.ViewModels.Home
@using AppMvc.Models.ViewModels.Friends
@using AppMvc.Models.ViewModels.Admin
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

---

### STEP 7: Copy Helper Classes

If AppRazor has helper classes (like `SeidoHelpers`), copy them to AppMvc:

```
AppRazor/SeidoHelpers/ → AppMvc/Helpers/
```

Update namespaces in the copied files from `AppRazor.SeidoHelpers` to `AppMvc.Helpers`.

---

## Common Patterns and Conversions

### Pattern 1: Simple GET Page

**Razor Pages:**
```csharp
public class IndexModel : PageModel
{
    public string Message { get; set; }
    
    public IActionResult OnGet()
    {
        Message = "Hello";
        return Page();
    }
}
```

**MVC:**
```csharp
public class HomeController : Controller
{
    public IActionResult Index()
    {
        var vm = new IndexViewModel
        {
            Message = "Hello"
        };
        return View(vm);
    }
}
```

### Pattern 2: POST with Form Data

**Razor Pages:**
```csharp
[BindProperty]
public string SearchTerm { get; set; }

public IActionResult OnPostSearch()
{
    // use SearchTerm
    return Page();
}
```

**MVC:**
```csharp
[HttpPost]
public IActionResult Search(string searchTerm)
{
    // use searchTerm
    return View();
}
// OR
[HttpPost]
public IActionResult Search(SearchViewModel model)
{
    // use model.SearchTerm
    return View(model);
}
```

### Pattern 3: Redirect

**Razor Pages:**
```csharp
return RedirectToPage("/Friends/Lists/FriendsList");
return RedirectToPage("/Friends/Detailview/EditFriend", new { id = friendId });
```

**MVC:**
```csharp
return RedirectToAction("List", "Friends");
return RedirectToAction("Edit", "Friends", new { id = friendId });
```

### Pattern 4: Route Parameters

**Razor Pages:**
```csharp
// URL: /Friends/Detailview/EditFriend?id=xxx
public async Task<IActionResult> OnGet()
{
    if (Guid.TryParse(Request.Query["id"], out Guid _friendId))
    {
        // use _friendId
    }
}
```

**MVC:**
```csharp
// URL: /Friends/Edit/xxx
[HttpGet]
public async Task<IActionResult> Edit(Guid? id)
{
    if (id.HasValue)
    {
        // use id.Value
    }
}
```

### Pattern 5: Model Binding

**Razor Pages:**
```csharp
[BindProperty]
public FriendIM FriendInput { get; set; }

public IActionResult OnPostSave()
{
    // FriendInput is automatically bound
}
```

**MVC:**
```csharp
[HttpPost]
public IActionResult Save(FriendIM friendInput)
{
    // friendInput is automatically bound from POST data
}
// OR bind entire ViewModel
[HttpPost]
public IActionResult Save(EditFriendViewModel model)
{
    // model.FriendInput is bound
}
```

---

## Testing and Validation

### Testing Checklist

#### For Each Migrated Page:

- [ ] **Navigation:** Links to the page work correctly
- [ ] **GET Request:** Page loads with correct data
- [ ] **Model Binding:** Data is displayed correctly
- [ ] **Form Submission:** POST requests work
- [ ] **Validation:** Client and server validation work
- [ ] **Redirects:** Post-submission redirects work
- [ ] **Error Handling:** Error pages display correctly
- [ ] **Routing:** URLs are clean and follow conventions

#### Testing Strategy:

1. **Unit Test Controllers:**
   ```csharp
   [Fact]
   public async Task List_ReturnsViewWithFriends()
   {
       // Arrange
       var controller = new FriendsController(...);
       
       // Act
       var result = await controller.List();
       
       // Assert
       var viewResult = Assert.IsType<ViewResult>(result);
       var model = Assert.IsType<FriendsListViewModel>(viewResult.Model);
       Assert.NotNull(model.FriendsList);
   }
   ```

2. **Integration Tests:**
   - Test full request/response cycle
   - Verify database interactions
   - Test authentication/authorization

3. **Manual Testing:**
   - Navigate through each page
   - Submit forms
   - Test pagination
   - Test search functionality
   - Test CRUD operations

---

## Migration Order (Recommended)

1. **Start with Simple Pages:**
   - Home/Index ✓
   - Home/Privacy ✓
   - Home/Error ✓

2. **Admin Pages:**
   - Admin/Seed

3. **Friends Overview Pages:**
   - Friends/Overview
   - Friends/FriendsAndPetsOverview
   - Friends/FriendsAndPetsCityOverview

4. **Friends List:**
   - Friends/List (with pagination and search)

5. **Friends Detail Pages:**
   - Friends/Details (read-only view)

6. **Friends Edit (Most Complex):**
   - Friends/Edit (create/edit with multiple sub-forms)

---

## Routing Configuration

### Default Route
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

### Custom Routes (if needed)
```csharp
app.MapControllerRoute(
    name: "friendsOverview",
    pattern: "Friends/Overview/{type?}",
    defaults: new { controller = "Friends", action = "Overview" });
```

### Attribute Routing (alternative)
```csharp
[Route("Friends")]
public class FriendsController : Controller
{
    [Route("")]
    [Route("List")]
    public async Task<IActionResult> List() { }
    
    [Route("Edit/{id?}")]
    public async Task<IActionResult> Edit(Guid? id) { }
}
```

---

## Common Issues and Solutions

### Issue 1: Model Binding Not Working
**Problem:** Form data not binding to model properties

**Solution:**
- Ensure form input names match model property names
- Use `asp-for` tag helpers
- Check that model properties are public with getters/setters

```cshtml
<!-- Correct -->
<input asp-for="FriendInput.FirstName" />

<!-- Manual (ensure name matches) -->
<input name="FriendInput.FirstName" />
```

### Issue 2: Multiple POST Handlers
**Problem:** Razor Pages had multiple OnPost* methods

**Solution:**
- Create separate controller actions for each handler
- Use distinct action names
- Update form asp-action attributes

### Issue 3: ViewData/TempData
**Problem:** Data not persisting across redirects

**Solution:**
- Use TempData for redirect scenarios
- Use strongly-typed ViewModels instead of ViewBag/ViewData

```csharp
// After POST, before redirect
TempData["SuccessMessage"] = "Friend saved successfully!";
return RedirectToAction("List");

// In target view
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success">@TempData["SuccessMessage"]</div>
}
```

### Issue 4: Partial Views
**Problem:** Shared components need to work in both patterns

**Solution:**
- Use ViewComponents for complex shared logic
- Use partial views for simple shared markup
- Ensure partial views use ViewModels, not PageModels

---

## Best Practices

### 1. ViewModel Design
- Create a ViewModel for each view
- Keep ViewModels in a logical folder structure
- Include only what the view needs
- Use ViewModels for both input and output

### 2. Controller Design
- Keep controllers thin
- Delegate business logic to services
- One controller per feature/entity
- Use async/await for I/O operations

### 3. View Design
- Use strongly-typed models
- Leverage Tag Helpers
- Keep logic minimal in views
- Use partial views and view components for reusability

### 4. Naming Conventions
- Controller: `{Feature}Controller` (e.g., FriendsController)
- Action: Verb or noun (e.g., List, Edit, Save, Delete)
- View: Matches action name (e.g., List.cshtml)
- ViewModel: `{Action}ViewModel` (e.g., EditFriendViewModel)

### 5. Dependency Injection
- Inject services into controllers via constructor
- Use interfaces for testability
- Register services in Program.cs

---

## Summary Conversion Table

| Razor Pages | MVC | Notes |
|-------------|-----|-------|
| PageModel | Controller | Contains action methods |
| OnGet() | [HttpGet] Action | GET request handler |
| OnPost*() | [HttpPost] Action | POST request handler |
| Page() | View() | Returns view |
| RedirectToPage() | RedirectToAction() | Redirects to another action |
| [BindProperty] | Action parameter | Model binding |
| asp-page | asp-controller/asp-action | Tag helpers |
| asp-page-handler | asp-action | Form handler |
| Pages/Feature/Page.cshtml | Views/Controller/Action.cshtml | File location |

---

## Conclusion

This migration requires systematic conversion of:
1. **PageModels** → **Controllers + Actions**
2. **Page Properties** → **ViewModels**
3. **Handler Methods** → **Action Methods**
4. **Page() returns** → **View() returns**
5. **View directives** → **MVC-compatible directives**
6. **Navigation links** → **Controller/Action routing**

The key is to maintain the same functionality while adapting to the MVC pattern's separation of concerns: Controllers handle requests and orchestrate logic, ViewModels transfer data, and Views render UI.

Start with simple pages to establish patterns, then progressively tackle more complex pages. Test thoroughly at each step to ensure functionality is preserved.
