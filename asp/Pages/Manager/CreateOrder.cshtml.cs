using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace asp.Pages.Manager;

public class CreateOrderModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateOrderModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public SelectList Clients { get; set; }
    public int? RecommendedMasterId { get; set; }
    public string? DeviceType { get; set; }
    public string? DeviceModel { get; set; }
    public string? ProblemDescription { get; set; }
    public string? Notes { get; set; }

    public void OnGet()
    {
        Clients = new SelectList(_context.Clients, "Id", "Name");
    }

    public async Task<IActionResult> OnPostAsync(int clientId, string deviceType, string deviceModel, string problemDescription, string notes, string action)
    {
        DeviceType = deviceType;
        DeviceModel = deviceModel;
        ProblemDescription = problemDescription;
        Notes = notes;

        if (action == "findMaster")
        {
            var result = await _context.Database
                .SqlQueryRaw<int>("SELECT repair_service_schema.get_least_loaded_employee()")
                .ToListAsync();

            if (result.Any())
            {
                RecommendedMasterId = result.First();
            }

            Clients = new SelectList(_context.Clients, "Id", "Name");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            Clients = new SelectList(_context.Clients, "Id", "Name");
            return Page();
        }

        var device = new Device
        {
            ClientId = clientId,
            Type = deviceType,
            Model = deviceModel,
            ReceivedDate = DateTime.Now
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

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
