using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace asp;

public enum PaymentType
{
    Предоплата,
    ПолнаяОплата,
    ЧастичнаяОплата
}

[Table("payments")]
public class Payment
{
    [Column("paymentid")]
    public int Id { get; set; }

    [Required]
    [Column("orderid")]
    public int OrderId { get; set; }
    public Order Order { get; set; }

    [Required]
    [Column("amount")]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Column("type")]
    public PaymentType Type { get; set; } = PaymentType.ПолнаяОплата;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("payment_date", TypeName = "timestamp without time zone")]
    public DateTime PaymentDate { get; set; } = DateTime.Now;
}
