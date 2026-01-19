using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp.Pages.Manager;

public class CreateClientModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateClientModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string name, string email, string phone, string? address)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Проверяем, не существует ли уже клиент с таким email
            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.Email == email);

            if (existingClient != null)
            {
                ModelState.AddModelError("email", "Клиент с таким email уже существует");
                return Page();
            }

            var client = new asp.Client
            {
                Name = name,
                Email = email,
                Phone = phone,
                Address = address,
                CreatedAt = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // Добавляем запись в лог активности
            var activityLog = new asp.ActivityLog
            {
                Action = $"Менеджер добавил нового клиента: {name} ({email})",
                Timestamp = DateTime.Now
            };

            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Клиент '{name}' успешно добавлен!";
            return RedirectToPage("/Manager/Clients");
        }
        catch (Exception ex)
        {
            // Логируем ошибку и показываем пользователю
            TempData["ErrorMessage"] = $"Произошла ошибка при сохранении клиента: {ex.Message}";
            return Page();
        }
    }
}
