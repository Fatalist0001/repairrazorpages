# RepairRazorPages

Веб-приложение на ASP.NET Core (Razor Pages) для управления сервисным центром по ремонту устройств (компьютеров, ноутбуков и оргтехники). Система автоматизирует процессы приёма заказов, диагностики, ремонта и отслеживания статусов с разделением по ролям: клиент, менеджер, мастер.

## Стек технологий

- **Backend:** ASP.NET Core 8.0 (Razor Pages)
- **ORM:** Entity Framework Core 8.0
- **База данных:** PostgreSQL 15+
- **Язык:** C# 12
- **Frontend:** Bootstrap 5, jQuery, Razor Pages (server-side rendering)

## Архитектура

```
asp/
├── Models/          # Entity models (Client, Order, Master, Device, Part, etc.)
├── Pages/           # Razor Pages
│   ├── Client/      # Клиентский интерфейс (просмотр заказов)
│   ├── Manager/     # Интерфейс менеджера (создание/редактирование заказов, клиентов)
│   ├── Master/      # Интерфейс мастера (диагностика, ремонт)
│   ├── RoleSelection/  # Выбор роли
│   ├── ActivityLog/    # Логи активности
│   ├── Shared/      # Layout и частичные представления
│   └── Index.cshtml # Главная (редирект на выбор роли)
├── Services/        # Бизнес-логика (OrderStatusService — управление статусами)
├── Middleware/      # ErrorHandlingMiddleware (глобальная обработка ошибок)
├── ApplicationDbContext.cs  # Контекст EF Core
├── Program.cs       # Точка входа, DI-контейнер, инициализация БД
├── appsettings.json # Конфигурация (строка подключения к БД)
└── asp.csproj       # Файл проекта (.NET 8.0)
```

## Модели данных

| Модель        | Таблица         | Описание                       |
|---------------|-----------------|--------------------------------|
| `Client`      | `clients`       | Клиенты сервисного центра      |
| `Order`       | `orders`        | Заказы на ремонт               |
| `Master`      | `employees`     | Сотрудники (мастера)           |
| `Device`      | `devices`       | Устройства клиентов            |
| `Part`        | `parts`         | Запасные части                 |
| `OrderPart`   | `orderparts`    | Связь заказов и запчастей      |
| `Payment`     | `payments`      | Платежи по заказам             |
| `ActivityLog` | `activitylog`   | Логи действий по заказам       |

## Функциональность по ролям

### Клиент
- Просмотр списка своих заказов
- Просмотр деталей заказа (статус, стоимость)

### Менеджер
- Создание и редактирование клиентов
- Приём устройств и создание заказов
- Управление статусами заказов
- Назначение мастера на заказ
- Добавление запчастей к заказу

### Мастер
- Просмотр назначенных заказов
- Проведение диагностики (указание неисправностей, предварительной стоимости)
- Выполнение ремонта
- Обновление статусов заказа

## Требования

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 15+
- Visual Studio 2022 / Rider / VS Code

## Установка и запуск

### 1. Клонирование репозитория

```bash
git clone https://github.com/Fatalist0001/repairrazorpages.git
cd repairrazorpages
```

### 2. Настройка базы данных

Создайте базу данных PostgreSQL и выполните SQL-скрипты:

```bash
psql -U postgres -f repair_service_db_script.sql
psql -U postgres -f repair_service_db_script_transaction.sql
psql -U postgres -f repair_service_db_script_transaction_testdata.sql
```

Скрипты создают:
- Базу данных `repair_service_db`
- Схему `repair_service_schema`
- Все необходимые таблицы
- Индексы и триггеры
- Функции для расчёта стоимости, логирования и балансировки нагрузки
- Тестовые данные (опционально)

### 3. Настройка строки подключения

Отредактируйте `asp/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=repair_service_db;Username=postgres;Password=your_password;SearchPath=repair_service_schema"
  }
}
```

### 4. Запуск приложения

```bash
cd asp
dotnet run
```

Приложение будет доступно по адресу `http://localhost:5000` или `https://localhost:5001`.

При первом запуске автоматически создаются тестовые данные (клиент, мастер, заказ).

## База данных

Подробная схема БД с триггерами и хранимыми процедурами:

| Компонент                 | Описание                                           |
|---------------------------|----------------------------------------------------|
| **Триггеры**              | Автообновление `updated_at`, корректировка склада, логирование статусов |
| **Функции**               | `calculate_order_total` (расчёт стоимости), `get_order_activity_log` (история), `get_least_loaded_employee` (балансировка) |
| **Индексы**               | По email, статусу, датам, наименованиям            |

### Дополнительные таблицы в схеме (через SQL-скрипты)
- `diagnostics` — информация о диагностике устройства
- `guarantee` — гарантийные обращения

## Разработка

### Структура проекта

```bash
repairrazorpages/
├── asp/                    # Основной проект
│   ├── Models/             # Модели данных
│   ├── Pages/              # Razor Pages
│   ├── Services/           # Сервисы
│   ├── Middleware/         # Middleware
│   ├── wwwroot/            # Статические файлы (CSS, JS, библиотеки)
│   └── appsettings.json    # Конфигурация
├── *.sql                   # SQL-скрипты для БД
├── asp.sln                 # Solution-файл
└── README.md               # Документация
```
