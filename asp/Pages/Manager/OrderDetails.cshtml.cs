using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace asp.Pages.Manager;

public class OrderDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public OrderDetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Order Order { get; set; }
    public SelectList Masters { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Order = await _context.Orders
            .Include(o => o.Client)
            .Include(o => o.Device)
            .Include(o => o.Master)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (Order == null)
        {
            return NotFound();
        }

        Masters = new SelectList(_context.Masters, "Id", "Name");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, string status, int? masterId, decimal? preliminaryCost, decimal? finalCost, string action, string? deviceType, string? deviceModel)
    {
        try
        {
            Order = await _context.Orders.FindAsync(id);
            if (Order == null)
            {
                TempData["ErrorMessage"] = "Заказ не найден.";
                return RedirectToPage(new { id });
            }

            switch (action)
            {
                case "update":
                    // Проверяем, существует ли мастер с указанным ID
                    if (masterId.HasValue)
                    {
                        var masterExists = await _context.Masters.AnyAsync(m => m.Id == masterId.Value);
                        if (!masterExists)
                        {
                            TempData["ErrorMessage"] = "Выбранный мастер не существует.";
                            return RedirectToPage(new { id });
                        }
                    }

                    Order.Status = status;
                    Order.MasterId = masterId;
                    Order.PreliminaryCost = preliminaryCost;
                    Order.FinalCost = finalCost;
                    Order.UpdatedAt = DateTime.Now;

                    // Update device if exists
                    if (Order.Device != null)
                    {
                        Order.Device.Type = deviceType;
                        Order.Device.Model = deviceModel;
                    }
                    break;
                case "sendCost":
                    // Logic to send cost to client (placeholder)
                    Order.Status = "Ожидает подтверждения клиента";
                    Order.UpdatedAt = DateTime.Now;
                    break;
                case "recordPayment":
                    // Placeholder for recording payment
                    break;
            }

            await _context.SaveChangesAsync();

            // Log activity
            var log = new asp.ActivityLog
            {
                OrderId = Order.Id,
                Action = $"Менеджер {action} для заказа {Order.Id}",
                Timestamp = DateTime.Now
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Изменения сохранены успешно!";
            return RedirectToPage(new { id });
        }
        catch (DbUpdateException dbEx)
        {
            TempData["ErrorMessage"] = $"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}";
            return RedirectToPage(new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Произошла ошибка при сохранении изменений: {ex.Message}";
            return RedirectToPage(new { id });
        }
    }
}
