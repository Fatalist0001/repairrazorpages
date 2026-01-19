using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp.Pages.Manager;

public class EditClientModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditClientModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public asp.Client? Client { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Client = await _context.Clients.FindAsync(Id);

        if (Client == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string name, string email, string phone, string? address)
    {
        try
        {
            var client = await _context.Clients.FindAsync(Id);

            if (client == null)
            {
                TempData["ErrorMessage"] = "Клиент не найден.";
                return RedirectToPage("/Manager/Clients");
            }

            if (!ModelState.IsValid)
            {
                Client = client;
                return Page();
            }

            // Проверяем, не занят ли email другим клиентом
            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.Email == email && c.Id != Id);

            if (existingClient != null)
            {
                ModelState.AddModelError("email", "Этот email уже используется другим клиентом");
                Client = client;
                return Page();
            }

            // Обновляем данные клиента
            client.Name = name;
            client.Email = email;
            client.Phone = phone;
            client.Address = address;

            await _context.SaveChangesAsync();

            // Добавляем запись в лог активности
            var activityLog = new asp.ActivityLog
            {
                Action = $"Менеджер изменил данные клиента: {name} ({email})",
                Timestamp = DateTime.Now
            };

            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Данные клиента '{name}' успешно обновлены!";
            return RedirectToPage("/Manager/Clients");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Произошла ошибка при сохранении изменений: {ex.Message}";
            return RedirectToPage("/Manager/Clients");
        }
    }
}
