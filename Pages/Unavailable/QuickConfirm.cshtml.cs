using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sitiowebb.Data;
using Sitiowebb.Models;

namespace Sitiowebb.Pages.Unavailable;

[Authorize]
public class QuickConfirmModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public QuickConfirmModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? Kind { get; set; }

    public string Title => $"Confirm {(Kind ?? "").ToLowerInvariant()}";

    public IActionResult OnGet()
    {
        if (string.IsNullOrWhiteSpace(Kind)) return RedirectToPage("/UnavailableOptions");
        var k = Kind!.Trim().ToLowerInvariant();
        if (k is not ("meeting" or "trip")) return RedirectToPage("/UnavailableOptions");
        return Page();
    }

    public async Task<IActionResult> OnPostYesAsync()
    {
        if (string.IsNullOrWhiteSpace(Kind)) return RedirectToPage("/UnavailableOptions");
        var k = (Kind ?? "").Trim().ToLowerInvariant();

        var userEmail = User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userEmail)) return RedirectToPage("/UnavailableOptions");

        var today = DateTime.UtcNow.Date;

        var entity = new Unavailability
        {
            UserEmail = userEmail,
            Kind = k,
            StartDate = today,
            EndDate = today
        };

        _db.Unavailabilities.Add(entity);
        await _db.SaveChangesAsync();

        // Aquí podrías notificar a managers si ya tienes mecanismo.

        return RedirectToPage("/UnavailableOptions");
    }
}
