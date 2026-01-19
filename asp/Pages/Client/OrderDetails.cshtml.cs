using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp.Pages.Client;

public class OrderDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public OrderDetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Order Order { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Order = await _context.Orders
            .Include(o => o.Client)
            .Include(o => o.Device)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (Order == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, string action, decimal? amount)
    {
        Order = await _context.Orders.FindAsync(id);
        if (Order == null)
        {
            return NotFound();
        }

        if (Order.Status != "Ожидает подтверждения клиента")
        {
            return BadRequest("Действие недоступно для текущего статуса заказа.");
        }

        switch (action)
        {
            case "agree":
                Order.Status = "Клиент согласился";
                break;
            case "decline":
                Order.Status = "Клиент отказался";
                break;
            case "prepay":
                if (amount.HasValue && amount > 0)
                {
                    Order.Prepayment = (Order.Prepayment ?? 0) + amount.Value;
                    // Возможно, добавить в payments, но таблица payments может быть другой
                }
                break;
            default:
                return BadRequest("Неизвестное действие.");
        }

        Order.UpdatedAt = DateTime.Now;

        // Логирование действия
        var log = new asp.ActivityLog
        {
            OrderId = Order.Id,
            Action = $"Клиент {action} на заказ {Order.Id}",
            Timestamp = DateTime.Now
        };
        _context.ActivityLogs.Add(log);

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }
}
