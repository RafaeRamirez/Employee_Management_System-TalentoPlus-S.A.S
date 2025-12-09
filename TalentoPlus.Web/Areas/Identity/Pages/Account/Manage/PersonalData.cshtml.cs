using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TalentoPlus.Infrastructure.Identity;

namespace TalentoPlus.Web.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class PersonalDataModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public PersonalDataModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostDownloadAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("No se pudo cargar al usuario.");
        }

        // Sugerencia: serializar los datos relevantes del usuario.
        var personalData = new Dictionary<string, string?>
        {
            ["Id"] = user.Id,
            ["Email"] = user.Email,
            ["UserName"] = user.UserName,
            ["PhoneNumber"] = user.PhoneNumber,
            ["Document"] = user.Document
        };

        var json = System.Text.Json.JsonSerializer.Serialize(personalData);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", "datos-personales.json");
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("No se pudo cargar al usuario.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "No se pudo eliminar la cuenta.");
            return Page();
        }

        await _signInManager.SignOutAsync();
        return Redirect("~/");
    }
}
