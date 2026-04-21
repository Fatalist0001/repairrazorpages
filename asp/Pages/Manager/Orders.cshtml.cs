using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp.Pages.Manager;

public class OrdersModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public OrdersModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public string StatusFilter { get; set; }
    public string ClientFilter { get; set; }
    public List<Order> Orders { get; set; }

    public async Task OnGetAsync(string status, string client)
    {
        StatusFilter = status;
        ClientFilter = client;

        var query = _context.Orders
            .Include(o => o.Client)
            .Include(o => o.Device)
            .Include(o => o.Master)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }

        if (!string.IsNullOrEmpty(client))
        {
            query = query.Where(o => o.Client.Name.Contains(client));
        }

        Orders = await query.ToListAsync();
    }

    public async Task<IActionResult> OnPostCalculateTotal(int orderId)
    {
        var total = await _context.Database
            .SqlQueryRaw<decimal>($"SELECT repair_service_schema.calculate_order_total({orderId})")
            .ToListAsync();

        if (total.Any())
        {
            TempData["TotalCost"] = $"Полная стоимость заказа #{orderId}: {total.First():C}";
        }

        return RedirectToPage();
    }
}
