using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asp.Pages.RoleSelection;

public class IndexModel : PageModel
{
    public IActionResult OnPost(string role)
    {
        if (!string.IsNullOrEmpty(role))
        {
            HttpContext.Session.SetString("UserRole", role);

            return role switch
            {
                "Client" => RedirectToPage("/Client/Index"),
                "Manager" => RedirectToPage("/Manager/Index"),
                "Master" => RedirectToPage("/Master/Index"),
                _ => Page()
            };
        }

        return Page();
    }
}
