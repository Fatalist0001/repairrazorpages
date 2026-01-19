using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp.Pages.Master;

public class OrdersModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public OrdersModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Order> Orders { get; set; } = new();
    public List<asp.Master> Masters { get; set; } = new();
    public int? SelectedMasterId { get; set; }

    public async Task OnGetAsync(int? masterId)
    {
        SelectedMasterId = masterId;
        Masters = await _context.Masters.ToListAsync();

        // Загружаем заказы в работе (диагностика и ремонт)
        var query = _context.Orders
            .Include(o => o.Client)
            .Include(o => o.Device)
            .Include(o => o.Master)
            .Where(o => o.Status == "диагностика" || o.Status == "ремонт");

        // Фильтруем по выбранному мастеру, если указан
        if (masterId.HasValue)
        {
            query = query.Where(o => o.MasterId == masterId.Value);
        }

        Orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}
