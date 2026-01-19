using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp.Pages.ActivityLog;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<asp.ActivityLog> ActivityLogs { get; set; } = new();

    public async Task OnGetAsync()
    {
        ActivityLogs = await _context.ActivityLogs
            .Include(a => a.Order)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}
