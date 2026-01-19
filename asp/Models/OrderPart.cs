using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace asp;

[Table("orderparts")]
public class OrderPart
{
    [Column("orderpartid")]
    public int Id { get; set; }

    [Required]
    [Column("orderid")]
    public int OrderId { get; set; }
    public Order Order { get; set; }

    [Required]
    [Column("partid")]
    public int PartId { get; set; }
    public Part Part { get; set; }

    [Required]
    [Column("quantity")]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    // Вычисляемое свойство - цена берется из связанной детали
    [NotMapped]
    public decimal UnitPrice => Part?.Price ?? 0;
}
