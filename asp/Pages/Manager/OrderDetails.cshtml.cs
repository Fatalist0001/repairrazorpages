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
    public List<ActivityLogItem> ActivityLogItems { get; set; } = new();

    public class ActivityLogItem
    {
        public DateTime Timestamp { get; set; }
        public string? EmployeeName { get; set; }
        public string? Action { get; set; }
    }

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
        Order = await _context.Orders
            .Include(o => o.Client)
            .Include(o => o.Device)
            .Include(o => o.Master)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (Order == null)
        {
            return NotFound();
        }

        if (action == "showLog")
        {
            var logData = await _context.Database
                .SqlQueryRaw<ActivityLogItem>($"SELECT log_timestamp as \"Timestamp\", employee_name as \"EmployeeName\", action as \"Action\" FROM repair_service_schema.get_order_activity_log({id})")
                .ToListAsync();

            ActivityLogItems = logData;
            Masters = new SelectList(_context.Masters, "Id", "Name");
            return Page();
        }

        try
        {
            Order.Status = status;
            Order.MasterId = masterId;
            Order.PreliminaryCost = preliminaryCost;
            Order.FinalCost = finalCost;
            Order.UpdatedAt = DateTime.Now;

            if (Order.Device != null)
            {
                Order.Device.Type = deviceType;
                Order.Device.Model = deviceModel;
            }

            await _context.SaveChangesAsync();

            var log = new asp.ActivityLog
            {
                OrderId = Order.Id,
                Action = $"Менеджер обновил заказ {Order.Id}",
                Timestamp = DateTime.Now
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Изменения сохранены успешно!";
            return RedirectToPage(new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
            Masters = new SelectList(_context.Masters, "Id", "Name");
            return Page();
        }
    }
}
