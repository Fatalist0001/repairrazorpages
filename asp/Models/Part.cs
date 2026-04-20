using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace asp;

[Table("parts")]
public class Part
{
    [Column("partid")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Column("unit_cost")]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Column("stock_quantity")]
    public int? StockQuantity { get; set; }

    [Column("reserved_quantity")]
    public int? ReservedQuantity { get; set; }

    [NotMapped]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation property
    public ICollection<OrderPart> OrderParts { get; set; } = new List<OrderPart>();
}
