using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp.Pages.Manager;

using Client = asp.Client;

public class ClientsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ClientsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Client> Clients { get; set; }

    public void OnGet()
    {
        Clients = _context.Clients.ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                TempData["ErrorMessage"] = "Клиент не найден.";
                return RedirectToPage();
            }

            // Проверяем, есть ли у клиента активные заказы
            var hasActiveOrders = await _context.Orders
                .AnyAsync(o => o.ClientId == id && o.Status != "отменён" && o.Status != "Ремонт завершён");

            if (hasActiveOrders)
            {
                TempData["ErrorMessage"] = "Нельзя удалить клиента с активными заказами.";
                return RedirectToPage();
            }

            _context.Clients.Remove(client);

            // Добавляем запись в лог активности
            var activityLog = new asp.ActivityLog
            {
                Action = $"Менеджер удалил клиента: {client.Name} ({client.Email})",
                Timestamp = DateTime.Now
            };

            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Клиент '{client.Name}' успешно удален.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Произошла ошибка при удалении клиента: {ex.Message}";
        }

        return RedirectToPage();
    }
}
