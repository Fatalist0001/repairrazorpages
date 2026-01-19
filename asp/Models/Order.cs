using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace asp;

public enum OrderStatus
{
    ПринятОтКлиента,
    ОжидаетДиагностики,
    ОжидаетСогласияКлиента,
    КлиентСогласился,
    КлиентОтказался,
    ВПроцессеРемонта,
    РемонтЗавершён
}

[Table("orders")]
public class Order
{
    [Column("orderid")]
    public int Id { get; set; }

    [Required]
    [Column("clientid")]
    public int ClientId { get; set; }
    public Client Client { get; set; }

    [Column("empid")]
    public int? MasterId { get; set; }
    public Master? Master { get; set; }

    [Column("deviceid")]
    public int? DeviceId { get; set; }
    public Device? Device { get; set; }

    [Column("status")]
    public string Status { get; set; } = "в обработке";

    [Column("diagnostic_cost")]
    public decimal? PreliminaryCost { get; set; }

    [Column("total_cost")]
    public decimal? FinalCost { get; set; }

    [Column("prepayment")]
    public decimal? Prepayment { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    // For compatibility, add missing properties
    [NotMapped]
    public string? DeviceType => Device?.Type;
    [NotMapped]
    public string? DeviceModel => Device?.Model;
    [NotMapped]
    public string ProblemDescription { get; set; } = "Unknown";
    [NotMapped]
    public string? Diagnosis { get; set; }
    [NotMapped]
    public string? RecommendedWork { get; set; }
    [NotMapped]
    public bool? RequiresPrepayment => Prepayment.HasValue;
    [NotMapped]
    public string? Notes { get; set; }

    // Navigation properties
    public ICollection<OrderPart> OrderParts { get; set; } = new List<OrderPart>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
