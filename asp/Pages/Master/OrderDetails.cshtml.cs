using asp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp.Pages.Master;

public class OrderDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public OrderDetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Order? Order { get; set; }
    public List<Part> AvailableParts { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // Загружаем заказ с деталями (для всех мастеров в работе)
        Order = await _context.Orders
            .Include(o => o.Client)
            .Include(o => o.Master)
            .Include(o => o.Device)
            .Include(o => o.OrderParts)
                .ThenInclude(op => op.Part)
            .FirstOrDefaultAsync(o => o.Id == Id &&
                                     (o.Status == "диагностика" || o.Status == "ремонт"));

        if (Order == null)
        {
            return NotFound();
        }

        // Загружаем доступные детали
        AvailableParts = await _context.Parts
            .Where(p => p.StockQuantity > 0 || p.StockQuantity == null)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitDiagnosisAsync(
        string diagnosis,
        string recommendedWork,
        decimal diagnosticCost,
        decimal? prepayment)
    {
        // Для демо-версии позволяем любому мастеру работать с заказами в работе
        // В реальном приложении нужно добавить аутентификацию и авторизацию

        // Валидация входных данных
        if (string.IsNullOrWhiteSpace(diagnosis))
        {
            ModelState.AddModelError("diagnosis", "Результаты диагностики обязательны для заполнения");
        }

        if (string.IsNullOrWhiteSpace(recommendedWork))
        {
            ModelState.AddModelError("recommendedWork", "Рекомендуемые работы обязательны для заполнения");
        }

        if (diagnosticCost <= 0)
        {
            ModelState.AddModelError("diagnosticCost", "Стоимость диагностики должна быть больше 0");
        }

        if (diagnosticCost > 100000)
        {
            ModelState.AddModelError("diagnosticCost", "Стоимость диагностики не может превышать 100 000 ₽");
        }

        if (prepayment.HasValue && prepayment.Value < 0)
        {
            ModelState.AddModelError("prepayment", "Предоплата не может быть отрицательной");
        }

        if (prepayment.HasValue && prepayment.Value > diagnosticCost)
        {
            ModelState.AddModelError("prepayment", "Предоплата не может превышать стоимость диагностики");
        }

        var order = await _context.Orders.FindAsync(Id);
        if (order == null || order.Status != "диагностика")
        {
            TempData["ErrorMessage"] = "Заказ не найден или недоступен для диагностики";
            return RedirectToPage();
        }

        if (!ModelState.IsValid)
        {
            // Перезагрузить данные для отображения
            Order = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Master)
                .Include(o => o.Device)
                .Include(o => o.OrderParts)
                    .ThenInclude(op => op.Part)
                .FirstOrDefaultAsync(o => o.Id == Id &&
                                         (o.Status == "диагностика" || o.Status == "ремонт"));

            AvailableParts = await _context.Parts
                .Where(p => p.StockQuantity > 0 || p.StockQuantity == null)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return Page();
        }

        // Проверяем допустимость перехода статуса
        var currentStatus = OrderStatusService.ParseStatus(order.Status);
        var newStatus = OrderStatusEnum.ОжидаетСогласияКлиента;

        if (!OrderStatusService.IsTransitionAllowed(currentStatus, newStatus))
        {
            TempData["ErrorMessage"] = $"Недопустимый переход статуса из '{OrderStatusService.StatusToString(currentStatus)}' в '{OrderStatusService.StatusToString(newStatus)}'";
            return RedirectToPage();
        }

        // Обновляем заказ с результатами диагностики
        order.Diagnosis = diagnosis;
        order.RecommendedWork = recommendedWork;
        order.PreliminaryCost = diagnosticCost;
        order.Prepayment = prepayment;
        order.Status = OrderStatusService.StatusToString(newStatus);
        order.UpdatedAt = DateTime.Now;

        // Добавляем запись в лог активности
        var activityLog = new asp.ActivityLog
        {
            OrderId = order.Id,
            Action = $"Мастер завершил диагностику. Стоимость: {diagnosticCost:C}" +
                    (prepayment.HasValue ? $", предоплата: {prepayment:C}" : ""),
            Timestamp = DateTime.Now
        };

        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Диагностика отправлена менеджеру для согласования с клиентом.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddPartAsync(int partId, int quantity)
    {
        if (partId <= 0)
        {
            TempData["ErrorMessage"] = "Выберите деталь";
            return RedirectToPage();
        }

        if (quantity <= 0)
        {
            TempData["ErrorMessage"] = "Количество должно быть больше 0";
            return RedirectToPage();
        }

        if (quantity > 100)
        {
            TempData["ErrorMessage"] = "Количество не может превышать 100 единиц";
            return RedirectToPage();
        }

        var order = await _context.Orders.FindAsync(Id);
        if (order == null || order.Status != "ремонт")
        {
            TempData["ErrorMessage"] = "Заказ не найден или недоступен для добавления деталей";
            return RedirectToPage();
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var part = await _context.Parts
                .FromSqlRaw("SELECT * FROM repair_service_schema.parts WHERE partid = {0} FOR UPDATE", partId)
                .FirstOrDefaultAsync();

            if (part == null)
            {
                TempData["ErrorMessage"] = "Деталь не найдена.";
                return RedirectToPage();
            }

            int availableQuantity = (part.StockQuantity ?? 0) - (part.ReservedQuantity ?? 0);
            if (availableQuantity <= 0 || availableQuantity < quantity)
            {
                TempData["ErrorMessage"] = $"Недостаточно деталей на складе. Доступно: {availableQuantity} шт.";
                return RedirectToPage();
            }

            part.ReservedQuantity = (part.ReservedQuantity ?? 0) + quantity;

            var existingOrderPart = await _context.OrderParts
                .FirstOrDefaultAsync(op => op.OrderId == Id && op.PartId == partId);

            if (existingOrderPart != null)
            {
                existingOrderPart.Quantity += quantity;
            }
            else
            {
                var orderPart = new OrderPart
                {
                    OrderId = Id,
                    PartId = partId,
                    Quantity = quantity
                };
                _context.OrderParts.Add(orderPart);
            }

            var activityLog = new asp.ActivityLog
            {
                OrderId = order.Id,
                Action = $"Добавлена деталь: {part.Name}, количество: {quantity}, цена: {part.Price:C}",
                Timestamp = DateTime.Now
            };

            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            TempData["SuccessMessage"] = $"Деталь '{part.Name}' добавлена к ремонту.";
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            TempData["ErrorMessage"] = $"Ошибка при добавлении детали: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCompleteRepairAsync()
    {
        var order = await _context.Orders.FindAsync(Id);
        if (order == null || order.Status != "ремонт")
        {
            TempData["ErrorMessage"] = "Заказ не найден или недоступен для завершения ремонта";
            return RedirectToPage();
        }

        var currentStatus = OrderStatusService.ParseStatus(order.Status);
        var newStatus = OrderStatusEnum.РемонтЗавершён;

        if (!OrderStatusService.IsTransitionAllowed(currentStatus, newStatus))
        {
            TempData["ErrorMessage"] = $"Недопустимый переход статуса из '{OrderStatusService.StatusToString(currentStatus)}' в '{OrderStatusService.StatusToString(newStatus)}'";
            return RedirectToPage();
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var orderParts = await _context.OrderParts
                .Include(op => op.Part)
                .Where(op => op.OrderId == Id)
                .ToListAsync();

            foreach (var orderPart in orderParts)
            {
                var part = await _context.Parts
                    .FromSqlRaw("SELECT * FROM repair_service_schema.parts WHERE partid = {0} FOR UPDATE", orderPart.PartId)
                    .FirstOrDefaultAsync();

                if (part == null) continue;

                part.StockQuantity = (part.StockQuantity ?? 0) - orderPart.Quantity;
                part.ReservedQuantity = (part.ReservedQuantity ?? 0) - orderPart.Quantity;
            }

            var partsCost = await _context.OrderParts
                .Where(op => op.OrderId == Id)
                .SumAsync(op => op.Quantity * op.UnitPrice);

            order.FinalCost = (order.PreliminaryCost ?? 0) + partsCost;
            order.Status = OrderStatusService.StatusToString(newStatus);
            order.UpdatedAt = DateTime.Now;

            var activityLog = new asp.ActivityLog
            {
                OrderId = order.Id,
                Action = $"Ремонт завершен. Итоговая стоимость: {order.FinalCost:C}",
                Timestamp = DateTime.Now
            };

            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            TempData["SuccessMessage"] = "Ремонт успешно завершен!";
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            TempData["ErrorMessage"] = $"Ошибка при завершении ремонта: {ex.Message}";
        }

        return RedirectToPage();
    }
}
