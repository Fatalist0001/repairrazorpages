using System.ComponentModel.DataAnnotations;

namespace asp.Services;

public enum OrderStatusEnum
{
    [Display(Name = "Принят от клиента")]
    ПринятОтКлиента,

    [Display(Name = "Ожидает диагностики")]
    ОжидаетДиагностики,

    [Display(Name = "Ожидает согласия клиента")]
    ОжидаетСогласияКлиента,

    [Display(Name = "Клиент согласился")]
    КлиентСогласился,

    [Display(Name = "Клиент отказался")]
    КлиентОтказался,

    [Display(Name = "В процессе ремонта")]
    ВПроцессеРемонта,

    [Display(Name = "Ремонт завершён")]
    РемонтЗавершён
}

public class OrderStatusService
{
    private static readonly Dictionary<OrderStatusEnum, List<OrderStatusEnum>> _allowedTransitions = new()
    {
        { OrderStatusEnum.ПринятОтКлиента, new List<OrderStatusEnum> { OrderStatusEnum.ОжидаетДиагностики } },
        { OrderStatusEnum.ОжидаетДиагностики, new List<OrderStatusEnum> { OrderStatusEnum.ОжидаетСогласияКлиента } },
        { OrderStatusEnum.ОжидаетСогласияКлиента, new List<OrderStatusEnum> { OrderStatusEnum.КлиентСогласился, OrderStatusEnum.КлиентОтказался } },
        { OrderStatusEnum.КлиентСогласился, new List<OrderStatusEnum> { OrderStatusEnum.ВПроцессеРемонта } },
        { OrderStatusEnum.ВПроцессеРемонта, new List<OrderStatusEnum> { OrderStatusEnum.РемонтЗавершён } },
        { OrderStatusEnum.КлиентОтказался, new List<OrderStatusEnum>() }, // Финальный статус
        { OrderStatusEnum.РемонтЗавершён, new List<OrderStatusEnum>() }   // Финальный статус
    };

    /// <summary>
    /// Проверяет, возможен ли переход из текущего статуса в новый
    /// </summary>
    public static bool IsTransitionAllowed(OrderStatusEnum currentStatus, OrderStatusEnum newStatus)
    {
        return _allowedTransitions.ContainsKey(currentStatus) &&
               _allowedTransitions[currentStatus].Contains(newStatus);
    }

    /// <summary>
    /// Возвращает список возможных следующих статусов для текущего статуса
    /// </summary>
    public static List<OrderStatusEnum> GetAllowedTransitions(OrderStatusEnum currentStatus)
    {
        return _allowedTransitions.ContainsKey(currentStatus)
            ? new List<OrderStatusEnum>(_allowedTransitions[currentStatus])
            : new List<OrderStatusEnum>();
    }

    /// <summary>
    /// Проверяет, является ли статус финальным (завершенным)
    /// </summary>
    public static bool IsFinalStatus(OrderStatusEnum status)
    {
        return status == OrderStatusEnum.КлиентОтказался || status == OrderStatusEnum.РемонтЗавершён;
    }

    /// <summary>
    /// Конвертирует строковое представление статуса в enum
    /// </summary>
    public static OrderStatusEnum ParseStatus(string statusString)
    {
        return statusString switch
        {
            "в обработке" => OrderStatusEnum.ПринятОтКлиента,
            "диагностика" => OrderStatusEnum.ОжидаетДиагностики,
            "ремонт" => OrderStatusEnum.ВПроцессеРемонта,
            "Ожидает подтверждения клиента" => OrderStatusEnum.ОжидаетСогласияКлиента,
            "отменён" => OrderStatusEnum.КлиентОтказался,
            _ => throw new ArgumentException($"Неизвестный статус: {statusString}")
        };
    }

    /// <summary>
    /// Конвертирует enum статуса в строковое представление
    /// </summary>
    public static string StatusToString(OrderStatusEnum status)
    {
        return status switch
        {
            OrderStatusEnum.ПринятОтКлиента => "Принят от клиента",
            OrderStatusEnum.ОжидаетДиагностики => "Ожидает диагностики",
            OrderStatusEnum.ОжидаетСогласияКлиента => "Ожидает согласия клиента",
            OrderStatusEnum.КлиентСогласился => "Клиент согласился",
            OrderStatusEnum.КлиентОтказался => "Клиент отказался",
            OrderStatusEnum.ВПроцессеРемонта => "В процессе ремонта",
            OrderStatusEnum.РемонтЗавершён => "Ремонт завершён",
            _ => throw new ArgumentException($"Неизвестный статус: {status}")
        };
    }

    /// <summary>
    /// Возвращает цвет Bootstrap для статуса
    /// </summary>
    public static string GetStatusBadgeClass(OrderStatusEnum status)
    {
        return status switch
        {
            OrderStatusEnum.ПринятОтКлиента => "primary",
            OrderStatusEnum.ОжидаетДиагностики => "warning",
            OrderStatusEnum.ОжидаетСогласияКлиента => "info",
            OrderStatusEnum.КлиентСогласился => "success",
            OrderStatusEnum.КлиентОтказался => "danger",
            OrderStatusEnum.ВПроцессеРемонта => "info",
            OrderStatusEnum.РемонтЗавершён => "success",
            _ => "secondary"
        };
    }
}
