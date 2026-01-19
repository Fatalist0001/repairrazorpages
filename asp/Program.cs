using asp;
using asp.Middleware;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddRazorPages();
builder.Services.AddSession();

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Check if data exists
    if (!context.Clients.Any())
    {
        var client = new Client
        {
            Name = "Иван Иванов",
            Email = "ivan@example.com",
            Phone = "+7 (123) 456-7890"
        };
        context.Clients.Add(client);
        context.SaveChanges(); // to get Id

        var master = new Master
        {
            Name = "Петр Петров",
            Phone = "+7 (987) 654-3210",
            Role = "Мастер",
            Specialization = "Ремонт компьютеров"
        };
        context.Masters.Add(master);
        context.SaveChanges();

        var order = new Order
        {
            ClientId = client.Id,
            MasterId = master.Id,
            Status = "Ожидает согласия клиента",
            PreliminaryCost = 5000,
            Prepayment = 0 // or null
        };
        context.Orders.Add(order);

        context.SaveChanges();
    }

    var tableNames = context.Database.SqlQuery<string>($"SELECT table_name FROM information_schema.tables WHERE table_schema = 'repair_service_schema' ORDER BY table_name").ToList();
    Console.WriteLine("Table names in repair_service_schema:");
    foreach (var name in tableNames)
    {
        Console.WriteLine(name);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Добавляем middleware для обработки ошибок
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
