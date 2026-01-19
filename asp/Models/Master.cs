using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace asp;

[Table("employees")]
public class Master
{
    [Column("empid")]
    public int Id { get; set; }

    [Required]
    [Column("full_name")]
    [StringLength(100)]
    public string Name { get; set; }

    [Column("phone")]
    [Phone]
    public string? Phone { get; set; }

    [Column("role")]
    public string? Role { get; set; }

    [Column("qualification")]
    public string? Specialization { get; set; }

    [Column("workload")]
    public int Workload { get; set; }

    // Navigation property
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
