
# Общее описание проекта

Этот проект представляет собой веб-приложение на базе ASP.NET Core Razor Pages для управления мастерской по ремонту компьютерной техники. Приложение позволяет управлять клиентами, заказами на ремонт, мастерами, запчастями, платежами и логировать действия пользователей. Оно разделено на роли: Клиент, Менеджер и Мастер, каждая из которых имеет свой интерфейс и функциональность.

### Архитектура приложения
- **Фреймворк**: ASP.NET Core Razor Pages
- **База данных**: PostgreSQL с использованием Entity Framework Core
- **Интерфейс**: Bootstrap для стилизации
- **Аутентификация**: Простая ролевая система на основе сессий (без полноценной аутентификации пользователей)

### Технологии и библиотеки
- **ASP.NET Core**: Веб-фреймворк для создания приложений
- **Entity Framework Core**: ORM для работы с базой данных
- **Npgsql.EntityFrameworkCore.PostgreSQL**: Провайдер PostgreSQL для EF Core
- **Razor Pages**: Фреймворк для создания веб-страниц
- **ASP.NET Core Sessions**: Для хранения состояния сессии
- **Bootstrap 5**: CSS-фреймворк для стилизации интерфейса
- **jQuery**: JavaScript библиотека для взаимодействия с DOM
- **jQuery Validation**: Плагин для валидации форм
- **jQuery Validation Unobtrusive**: Неинтрузивная валидация форм
- **Microsoft.Extensions.Logging**: Система логирования

### Основные компоненты
1. **Модели данных** (Entity Framework Core)
2. **Сервисы** для бизнес-логики
3. **Middleware** для обработки ошибок
4. **Razor Pages** для пользовательского интерфейса
5. **Конфигурация** приложения

## Подробное описание по компонентам

### 1. Модели данных (Models/)

#### asp/Models/ApplicationDbContext.cs
Класс контекста базы данных, наследующий от `DbContext`. Определяет `DbSet` для всех основных сущностей:
- Clients
- Orders
- Masters
- Devices
- Parts
- OrderParts
- ActivityLogs
- Payments

Функциональность: Управление подключением к базе данных и выполнение CRUD операций.

#### asp/Models/Client.cs
Модель клиента с атрибутами:
- Id (int) - первичный ключ
- Name (string) - ФИО, обязательное
- Email (string) - email, обязательное, с валидацией
- Phone (string) - телефон, обязательное, с валидацией
- Address (string?) - адрес, опциональное
- CreatedAt (DateOnly) - дата регистрации, по умолчанию текущая дата

Навигационное свойство: Orders (коллекция заказов клиента)

#### asp/Models/Device.cs
Модель устройства с атрибутами:
- Id (int) - первичный ключ
- ClientId (int) - внешний ключ на клиента
- Client (navigation) - ссылка на клиента
- Type (string) - тип устройства, обязательное
- Model (string?) - модель устройства
- SerialNumber (string?) - серийный номер
- InitialCondition (string?) - начальное состояние
- ReceivedDate (DateTime) - дата приема, по умолчанию текущая

#### asp/Models/Master.cs
Модель мастера (сотрудника) с атрибутами:
- Id (int) - первичный ключ
- Name (string) - ФИО, обязательное
- Phone (string?) - телефон
- Role (string?) - роль
- Specialization (string?) - специализация
- Workload (int) - нагрузка

Навигационное свойство: Orders (коллекция заказов мастера)

#### asp/Models/Order.cs
Модель заказа с атрибутами:
- Id (int) - первичный ключ
- ClientId (int) - внешний ключ на клиента
- Client (navigation) - ссылка на клиента
- MasterId (int?) - внешний ключ на мастера
- Master (navigation) - ссылка на мастера
- DeviceId (int?) - внешний ключ на устройство
- Device (navigation) - ссылка на устройство
- Status (string) - статус заказа, по умолчанию "в обработке"
- PreliminaryCost (decimal?) - стоимость диагностики
- FinalCost (decimal?) - общая стоимость
- Prepayment (decimal?) - предоплата
- CreatedAt (DateTime) - дата создания
- UpdatedAt (DateTime?) - дата обновления

Навигационные свойства: OrderParts, ActivityLogs, Payments

Дополнительные вычисляемые свойства (NotMapped):
- DeviceType, DeviceModel, ProblemDescription, Diagnosis, RecommendedWork, RequiresPrepayment, Notes

#### OrderStatus Enum
Перечисление статусов заказа:
- ПринятОтКлиента
- ОжидаетДиагностики
- ОжидаетСогласияКлиента
- КлиентСогласился
- КлиентОтказался
- ВПроцессеРемонта
- РемонтЗавершён

#### asp/Models/Part.cs
Модель запчасти с атрибутами:
- Id (int) - первичный ключ
- Name (string) - название, обязательное
- Description (string?) - описание
- Price (decimal) - цена, обязательное, больше 0
- StockQuantity (int?) - количество на складе
- CreatedAt (DateTime) - дата создания

Навигационное свойство: OrderParts

#### asp/Models/OrderPart.cs
Связующая модель между заказом и запчастью:
- Id (int) - первичный ключ
- OrderId (int) - внешний ключ на заказ
- Order (navigation) - ссылка на заказ
- PartId (int) - внешний ключ на запчасть
- Part (navigation) - ссылка на запчасть
- Quantity (int) - количество, обязательное, больше 0
- AddedAt (DateTime) - дата добавления

Вычисляемое свойство: UnitPrice (цена из связанной запчасти)

#### asp/Models/ActivityLog.cs
Модель лога активности:
- Id (int) - первичный ключ
- OrderId (int?) - внешний ключ на заказ
- Order (navigation) - ссылка на заказ
- EmployeeId (int?) - ID сотрудника (менеджер или мастер)
- Action (string) - описание действия
- Timestamp (DateTime) - время действия

#### asp/Models/Payment.cs
Модель платежа:
- Id (int) - первичный ключ
- OrderId (int) - внешний ключ на заказ
- Order (navigation) - ссылка на заказ
- Amount (decimal) - сумма, обязательное, больше 0
- Type (PaymentType) - тип оплаты
- Notes (string?) - примечания
- PaymentDate (DateTime) - дата платежа

#### PaymentType Enum
Перечисление типов оплаты:
- Предоплата
- ПолнаяОплата
- ЧастичнаяОплата

### 2. Сервисы (Services/)

#### asp/Services/OrderStatusService.cs
Статический класс для управления статусами заказов.

Методы:
- IsTransitionAllowed(OrderStatusEnum current, OrderStatusEnum new) - проверяет допустимость перехода
- GetAllowedTransitions(OrderStatusEnum current) - возвращает список допустимых следующих статусов
- IsFinalStatus(OrderStatusEnum status) - проверяет, является ли статус финальным
- ParseStatus(string statusString) - конвертирует строку в enum
- StatusToString(OrderStatusEnum status) - конвертирует enum в строку
- GetStatusBadgeClass(OrderStatusEnum status) - возвращает CSS класс для Bootstrap badge

Внутренний словарь _allowedTransitions определяет допустимые переходы между статусами.

### 3. Middleware (Middleware/)

#### asp/Middleware/ErrorHandlingMiddleware.cs
Middleware для глобальной обработки исключений.

Функциональность:
- Перехватывает все необработанные исключения
- Логирует ошибки
- Возвращает JSON ответ с сообщением об ошибке
- Устанавливает статус код 500

### 4. Razor Pages (Pages/)

#### asp/Pages/RoleSelection/Index.cshtml
Страница выбора роли пользователя.

Разметка:
- Заголовок "Мастерская по ремонту компьютеров"
- Три кнопки: "Войти как Клиент", "Войти как Менеджер", "Войти как Мастер"
- Форма с методом POST отправляет выбранную роль

#### asp/Pages/RoleSelection/Index.cshtml.cs
Обработчик страницы выбора роли.

Метод OnPost(string role):
- Сохраняет роль в сессии (HttpContext.Session.SetString("UserRole", role))
- Перенаправляет на соответствующую страницу в зависимости от роли

#### asp/Pages/Manager/Index.cshtml
Дашборд менеджера.

Разметка:
- Приветствие "Добро пожаловать, Менеджер!"
- Три карточки с ссылками:
  - Список заказов (/Manager/Orders)
  - Управление клиентами (/Manager/Clients)
  - Лог активности (/ActivityLog)

#### asp/Pages/Manager/Index.cshtml.cs
Простая страница без дополнительной логики (только OnGet()).

#### asp/Pages/Manager/Orders.cshtml
Страница списка заказов менеджера.

Разметка:
- Кнопки: "Создать заказ", "Управление клиентами"
- Форма фильтрации по статусу и клиенту
- Таблица заказов с колонками: №, Клиент, Устройство, Статус, Мастер, Дата создания, Действия
- Ссылка на детали заказа для каждого заказа

#### asp/Pages/Manager/Orders.cshtml.cs
Модель страницы списка заказов.

Свойства:
- StatusFilter, ClientFilter - фильтры
- Orders - список заказов

Метод OnGetAsync(string status, string client):
- Применяет фильтры к запросу
- Включает связанные данные (Client, Master)
- Возвращает отфильтрованный список заказов

#### asp/Pages/Manager/CreateOrder.cshtml
Форма создания нового заказа.

Разметка:
- Поля: Клиент (select), Тип устройства, Модель устройства, Описание проблемы, Примечания
- Кнопки: "Создать заказ (отправить на диагностику)", "Создать заказ (сразу на ремонт)"

#### asp/Pages/Manager/CreateOrder.cshtml.cs
Обработчик создания заказа.

Свойства:
- Clients (SelectList) - список клиентов для выбора

Метод OnGet():
- Загружает список клиентов

Метод OnPostAsync(...):
- Создает новое устройство
- Создает новый заказ со ссылкой на устройство
- Устанавливает статус в зависимости от действия (диагностика или ремонт)
- Сохраняет в базу данных
- Перенаправляет на список заказов

#### asp/Pages/Manager/OrderDetails.cshtml
Детальная страница заказа с вкладками.

Разметка:
- Вкладки: "Основное", "Детали ремонта"
- Вкладка "Основное":
  - Информация о клиенте и устройстве
  - Форма изменения статуса, мастера, стоимости диагностики, общей стоимости
  - Кнопки: "Сохранить изменения", "Отправить стоимость клиенту", "Записать оплату"
- Вкладка "Детали ремонта": placeholder для деталей ремонта

#### asp/Pages/Manager/OrderDetails.cshtml.cs
Обработчик детальной страницы заказа.

Свойства:
- Order - текущий заказ
- Masters (SelectList) - список мастеров

Метод OnGetAsync(int id):
- Загружает заказ с связанными данными
- Возвращает 404 если не найден

Метод OnPostAsync(int id, ...):
- Обновляет заказ в зависимости от действия (update, sendCost, recordPayment)
- Логирует действия в ActivityLog
- Обрабатывает ошибки и показывает сообщения

### 5. Конфигурация

#### asp/Program.cs
Основной файл конфигурации приложения.

Настройки:
- Добавление DbContext с PostgreSQL провайдером
- Добавление Razor Pages
- Добавление сессий
- Middleware для обработки ошибок
- Seeding начальных данных (клиент, мастер, заказ)
- Вывод списка таблиц в схеме базы данных

#### asp/appsettings.json
Файл конфигурации.

Содержит:
- ConnectionString для PostgreSQL
- Настройки логирования
- AllowedHosts: "*"
