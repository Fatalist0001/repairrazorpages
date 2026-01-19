using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _context;

    public List<string> TableNames { get; set; } = new();

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public void OnGet()
    {
        TableNames = _context.Database.SqlQuery<string>($"SELECT table_name FROM information_schema.tables WHERE table_schema = 'repair_service_schema' ORDER BY table_name").ToList();
    }
}
