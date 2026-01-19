using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace asp;

[Table("activitylog")]
public class ActivityLog
{
    [Column("logid")]
    public int Id { get; set; }

    [Column("orderid")]
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    [Column("empid")]
    public int? EmployeeId { get; set; } // Can be Manager or Master ID

    [Required]
    [Column("action")]
    [StringLength(500)]
    public string Action { get; set; }

    [Column("timestamp", TypeName = "timestamp without time zone")]
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
