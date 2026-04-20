-- Скрипт создания базы данных repair_service_db для системы управления ремонтом устройств
-- Актуальная версия с поддержкой транзакций для резервирования деталей

-- Создание базы данных
CREATE DATABASE repair_service_db
ENCODING 'UTF8'
LC_COLLATE 'ru_RU.UTF-8'
LC_CTYPE 'ru_RU.UTF-8';

-- Подключение к базе данных
\c repair_service_db

-- Создание схемы
CREATE SCHEMA repair_service_schema;

-- Установка search_path для всей сессии
SET search_path TO repair_service_schema, public;

-- Создание таблицы clients
CREATE TABLE repair_service_schema.clients (
    clientid SERIAL PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20) UNIQUE,
    email VARCHAR(100) UNIQUE,
    address TEXT,
    registration_date DATE DEFAULT CURRENT_DATE
);

-- Создание таблицы employees
CREATE TABLE repair_service_schema.employees (
    empid SERIAL PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    role VARCHAR(50),
    qualification VARCHAR(100),
    workload INTEGER DEFAULT 0
);

-- Создание таблицы devices
CREATE TABLE repair_service_schema.devices (
    deviceid SERIAL PRIMARY KEY,
    clientid INTEGER NOT NULL REFERENCES repair_service_schema.clients(clientid) ON DELETE CASCADE,
    type VARCHAR(50) NOT NULL,
    model VARCHAR(100),
    serial_number VARCHAR(100) UNIQUE,
    initial_condition TEXT,
    received_date DATE DEFAULT CURRENT_DATE
);

-- Создание таблицы orders
CREATE TABLE repair_service_schema.orders (
    orderid SERIAL PRIMARY KEY,
    clientid INTEGER NOT NULL REFERENCES repair_service_schema.clients(clientid) ON DELETE CASCADE,
    empid INTEGER REFERENCES repair_service_schema.employees(empid) ON DELETE SET NULL,
    deviceid INTEGER REFERENCES repair_service_schema.devices(deviceid) ON DELETE CASCADE,
    status VARCHAR(50) DEFAULT 'в обработке',
    diagnostic_cost NUMERIC(10,2),
    total_cost NUMERIC(10,2),
    prepayment NUMERIC(10,2),
    created_at TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Создание таблицы diagnostics
CREATE TABLE repair_service_schema.diagnostics (
    diagnosticid SERIAL PRIMARY KEY,
    orderid INTEGER NOT NULL REFERENCES repair_service_schema.orders(orderid) ON DELETE CASCADE,
    empid INTEGER REFERENCES repair_service_schema.employees(empid) ON DELETE SET NULL,
    deviceid INTEGER REFERENCES repair_service_schema.devices(deviceid) ON DELETE CASCADE,
    diagnostic_date DATE DEFAULT CURRENT_DATE,
    problems TEXT,
    estimated_cost NUMERIC(10,2),
    requires_prepayment BOOLEAN DEFAULT FALSE
);

-- Создание таблицы activitylog
CREATE TABLE repair_service_schema.activitylog (
    logid SERIAL PRIMARY KEY,
    orderid INTEGER NOT NULL REFERENCES repair_service_schema.orders(orderid) ON DELETE CASCADE,
    empid INTEGER REFERENCES repair_service_schema.employees(empid) ON DELETE SET NULL,
    timestamp TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    action TEXT
);

-- Создание таблицы parts
CREATE TABLE repair_service_schema.parts (
    partid SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    unit_cost NUMERIC(10,2),
    stock_quantity INTEGER DEFAULT 0,
    reserved_quantity INTEGER DEFAULT 0
);

-- Создание таблицы orderparts
CREATE TABLE repair_service_schema.orderparts (
    orderpartid SERIAL PRIMARY KEY,
    orderid INTEGER NOT NULL REFERENCES repair_service_schema.orders(orderid) ON DELETE CASCADE,
    partid INTEGER NOT NULL REFERENCES repair_service_schema.parts(partid) ON DELETE CASCADE ON UPDATE CASCADE,
    quantity INTEGER DEFAULT 1
);

-- Создание таблицы guarantee
CREATE TABLE repair_service_schema.guarantee (
    guaranteeid SERIAL PRIMARY KEY,
    orderid INTEGER NOT NULL REFERENCES repair_service_schema.orders(orderid) ON DELETE CASCADE,
    problem_description TEXT,
    complaint_date DATE DEFAULT CURRENT_DATE,
    resolution_date DATE,
    compensation_amount NUMERIC(10,2)
);

-- Создание таблицы payments
CREATE TABLE repair_service_schema.payments (
    paymentid SERIAL PRIMARY KEY,
    orderid INTEGER NOT NULL REFERENCES repair_service_schema.orders(orderid) ON DELETE CASCADE,
    amount NUMERIC(10,2) NOT NULL CHECK (amount > 0),
    payment_date DATE DEFAULT CURRENT_DATE,
    payment_type VARCHAR(50)
);

-- Создание индексов
CREATE INDEX idx_clients_email ON repair_service_schema.clients(email);
CREATE INDEX idx_clients_registration_date ON repair_service_schema.clients(registration_date);
CREATE INDEX idx_employees_role ON repair_service_schema.employees(role);
CREATE INDEX idx_devices_type ON repair_service_schema.devices(type);
CREATE INDEX idx_devices_serial_number ON repair_service_schema.devices(serial_number);
CREATE INDEX idx_orders_status ON repair_service_schema.orders(status);
CREATE INDEX idx_orders_created_at ON repair_service_schema.orders(created_at);
CREATE INDEX idx_parts_name ON repair_service_schema.parts(name);
CREATE INDEX idx_payments_payment_date ON repair_service_schema.payments(payment_date);

-- Триггер для автоматического обновления updated_at в orders
CREATE OR REPLACE FUNCTION repair_service_schema.update_order_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_update_order_timestamp
BEFORE UPDATE ON repair_service_schema.orders
FOR EACH ROW EXECUTE FUNCTION repair_service_schema.update_order_timestamp();

-- Триггер для логирования изменения статуса заказа
CREATE OR REPLACE FUNCTION repair_service_schema.log_order_status_change()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.status IS DISTINCT FROM OLD.status THEN
        INSERT INTO repair_service_schema.activitylog(orderid, empid, action)
        VALUES (NEW.orderid, NEW.empid, 'Статус изменён на: ' || NEW.status);
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_log_order_status_change
AFTER UPDATE ON repair_service_schema.orders
FOR EACH ROW EXECUTE FUNCTION repair_service_schema.log_order_status_change();

-- Функция расчета полной стоимости заказа
CREATE OR REPLACE FUNCTION repair_service_schema.calculate_order_total(order_id INTEGER)
RETURNS NUMERIC
LANGUAGE plpgsql
AS $$
DECLARE
    parts_sum NUMERIC;
    diag_cost NUMERIC;
BEGIN
    SELECT COALESCE(SUM(p.unit_cost * op.quantity), 0)
    INTO parts_sum
    FROM repair_service_schema.orderparts op
    JOIN repair_service_schema.parts p ON op.partid = p.partid
    WHERE op.orderid = order_id;

    SELECT COALESCE(diagnostic_cost, 0)
    INTO diag_cost
    FROM repair_service_schema.orders
    WHERE orderid = order_id;

    RETURN parts_sum + diag_cost;
END;
$$;

-- Функция получения истории действий по заказу
CREATE OR REPLACE FUNCTION repair_service_schema.get_order_activity_log(order_id INTEGER)
RETURNS TABLE(
    log_timestamp TIMESTAMP WITHOUT TIME ZONE,
    employee_name VARCHAR(100),
    action TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT al.timestamp, e.full_name, al.action
    FROM repair_service_schema.activitylog al
    LEFT JOIN repair_service_schema.employees e ON al.empid = e.empid
    WHERE al.orderid = order_id
    ORDER BY al.timestamp DESC;
END;
$$;

-- Функция получения наименее загруженного сотрудника
CREATE OR REPLACE FUNCTION repair_service_schema.get_least_loaded_employee()
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
    result_empid INTEGER;
BEGIN
    SELECT e.empid
    INTO result_empid
    FROM repair_service_schema.employees e
    LEFT JOIN (
        SELECT empid, COUNT(*) AS active_count
        FROM repair_service_schema.orders
        WHERE status NOT IN ('завершён', 'отменён')
        GROUP BY empid
    ) o ON e.empid = o.empid
    ORDER BY COALESCE(o.active_count, 0) ASC, e.empid ASC
    LIMIT 1;

    RETURN result_empid;
END;
$$;

-- ==================== ТЕСТОВЫЕ ДАННЫЕ ====================

-- Клиенты
INSERT INTO repair_service_schema.clients (full_name, phone, email, address) VALUES
('Тестовый Клиент', '+79001234567', 'test@test.com', 'Тестовый адрес'),
('Иванов Иван', '+79012345678', 'ivanov@test.com', 'ул. Пушкина 10'),
('Петров Петр', '+79023456789', 'petrov@test.com', 'ул. Ленина 5');

-- Устройства
INSERT INTO repair_service_schema.devices (clientid, type, model, serial_number, initial_condition) VALUES
(1, 'смартфон', 'iPhone 14', 'SN123456789', 'Разбит экран'),
(1, 'компьютер', 'ПК Dell Optiplex', 'PC123456', 'Не включается'),
(2, 'смартфон', 'Samsung Galaxy S21', 'SN987654321', 'Не заряжается'),
(3, 'ноутбук', 'HP Pavilion', 'SN111222333', 'Перегревается');

-- Сотрудники (мастера)
INSERT INTO repair_service_schema.employees (full_name, phone, role, qualification) VALUES
('Мастер Алексей', '+79001112233', 'мастер', 'Ремонт смартфонов'),
('Мастер Сергей', '+79004445566', 'мастер', 'Ремонт компьютеров');

-- Детали (склад) - с поддержкой транзакций
INSERT INTO repair_service_schema.parts (name, description, unit_cost, stock_quantity, reserved_quantity) VALUES
('Дисплей iPhone 14', 'Оригинальный дисплей', 15000.00, 5, 0),
('Аккумулятор iPhone 13', 'Оригинальный аккумулятор', 3500.00, 10, 0),
('Экран Samsung S21', 'OLED экран', 8000.00, 3, 0),
('Камера iPhone 12', 'Основная камера', 5000.00, 8, 0),
('Разъём зарядки USB-C', 'Разъём зарядки', 500.00, 20, 0),
('SSD 500GB', 'Накопитель SSD', 4500.00, 10, 0),
('Оперативная память 8GB DDR4', 'Планка RAM', 2500.00, 15, 0),
('Оперативная память 16GB DDR4', 'Планка RAM', 4500.00, 10, 0),
('Жёсткий диск 1TB', 'HDD', 3500.00, 8, 0),
('Блок питания 500W', 'БП', 3000.00, 5, 0);

-- Заказы в статусе "ремонт" для тестирования транзакций
INSERT INTO repair_service_schema.orders (clientid, deviceid, status, diagnostic_cost) VALUES
(1, 1, 'ремонт', 5000.00),
(1, 1, 'ремонт', 3000.00),
(1, 2, 'ремонт', 2000.00),
(1, 2, 'ремонт', 2500.00),
(2, 3, 'ремонт', 1500.00),
(3, 4, 'ремонт', 4000.00);