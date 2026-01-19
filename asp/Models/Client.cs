using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace asp;

[Table("clients")]
public class Client
{
    [Column("clientid")]
    public int Id { get; set; }

    [Required]
    [Column("full_name")]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [Column("email")]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [Column("phone")]
    [RegularExpression(@"^\+7\d{10}$", ErrorMessage = "Номер телефона должен быть в формате +7XXXXXXXXXX")]
    public string Phone { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("registration_date")]
    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    // Navigation property
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
