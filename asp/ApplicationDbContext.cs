using Microsoft.EntityFrameworkCore;

namespace asp;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Master> Masters { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<OrderPart> OrderParts { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<Payment> Payments { get; set; }
}
