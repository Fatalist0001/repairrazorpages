using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace asp.Pages.Manager;

public class CreateOrderModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateOrderModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public SelectList Clients { get; set; }

    public void OnGet()
    {
        Clients = new SelectList(_context.Clients, "Id", "Name");
    }

    public async Task<IActionResult> OnPostAsync(int clientId, string deviceType, string deviceModel, string problemDescription, string notes, string action)
    {
        if (!ModelState.IsValid)
        {
            Clients = new SelectList(_context.Clients, "Id", "Name");
            return Page();
        }

        // Создаем устройство
        var device = new Device
        {
            ClientId = clientId,
            Type = deviceType,
            Model = deviceModel,
            ReceivedDate = DateTime.Now
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        // Создаем заказ с ссылкой на устройство
        var order = new Order
        {
            ClientId = clientId,
            DeviceId = device.Id,
            ProblemDescription = problemDescription,
            Notes = notes,
            Status = action == "diagnostic" ? "диагностика" : "ремонт",
            CreatedAt = DateTime.Now
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Manager/Orders");
    }
}
