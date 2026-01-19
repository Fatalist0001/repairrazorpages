using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace asp.Pages.Client;

public class OrdersModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public OrdersModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public string PhoneFilter { get; set; }
    public string EmailFilter { get; set; }
    public List<Order> Orders { get; set; }

    public async Task OnGetAsync(string phone, string email)
    {
        PhoneFilter = phone;
        EmailFilter = email;

        string normalizedPhone = null;
        if (!string.IsNullOrEmpty(phone))
        {
            normalizedPhone = NormalizePhoneNumber(phone);
        }

        if (!string.IsNullOrEmpty(normalizedPhone) || !string.IsNullOrEmpty(email))
        {
            var query = _context.Orders.Include(o => o.Client).Include(o => o.Device).AsQueryable();

            if (!string.IsNullOrEmpty(normalizedPhone))
            {
                query = query.Where(o => o.Client.Phone.Contains(normalizedPhone));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(o => o.Client.Email.Contains(email));
            }

            Orders = await query.ToListAsync();
        }
    }

    private string NormalizePhoneNumber(string phone)
    {
        // Удалить все нецифровые символы кроме +
        var digits = Regex.Replace(phone, @"[^\d\+]", "");

        if (digits.StartsWith("+7") && digits.Length == 12)
        {
            return digits;
        }
        else if (digits.StartsWith("8") && digits.Length == 11)
        {
            return "+7" + digits.Substring(1);
        }
        else if (digits.StartsWith("7") && digits.Length == 11)
        {
            return "+7" + digits.Substring(1);
        }
        else if (digits.Length == 10)
        {
            return "+7" + digits;
        }
        else if (digits.Length == 11 && digits.StartsWith("7"))
        {
            return "+" + digits;
        }
        // Если не подходит, вернуть null или оригинал
        return null;
    }
}
