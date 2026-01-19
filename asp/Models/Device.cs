using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace asp;

[Table("devices")]
public class Device
{
    [Column("deviceid")]
    public int Id { get; set; }

    [Column("clientid")]
    public int ClientId { get; set; }
    public Client Client { get; set; }

    [Required]
    [Column("type")]
    [StringLength(50)]
    public string Type { get; set; }

    [Column("model")]
    [StringLength(100)]
    public string? Model { get; set; }

    [Column("serial_number")]
    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [Column("initial_condition")]
    public string? InitialCondition { get; set; }

    [Column("received_date", TypeName = "timestamp without time zone")]
    public DateTime ReceivedDate { get; set; } = DateTime.Now;
}
