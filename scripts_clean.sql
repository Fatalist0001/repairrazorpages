SET search_path TO repair_service_schema, public;

CREATE TABLE repair_service_schema.employees (
    empid SERIAL PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    role VARCHAR(50),
    qualification TEXT
);

CREATE TABLE repair_service_schema.clients (
    clientid SERIAL PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(100),
    address TEXT,
    registration_date DATE DEFAULT CURRENT_DATE
);

CREATE TABLE repair_service_schema.devices (
    deviceid SERIAL PRIMARY KEY,
    clientid INTEGER REFERENCES repair_service_schema.clients(clientid) ON DELETE CASCADE,
    type VARCHAR(50) NOT NULL,
    model VARCHAR(100) NOT NULL,
    serial_number VARCHAR(50) UNIQUE,
    initial_condition TEXT,
    received_date DATE DEFAULT CURRENT_DATE
);

CREATE TABLE repair_service_schema.orders (
    orderid SERIAL PRIMARY KEY,
    clientid INTEGER REFERENCES repair_service_schema.clients(clientid),
    empid INTEGER REFERENCES repair_service_schema.employees(empid),
    deviceid INTEGER REFERENCES repair_service_schema.devices(deviceid),
    status VARCHAR(50) DEFAULT 'в обработке',
    diagnostic_cost DECIMAL(10, 2),
    total_cost DECIMAL(10, 2),
    prepayment DECIMAL(10, 2) DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE repair_service_schema.parts (
    partid SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    unit_cost DECIMAL(10, 2) NOT NULL,
    stock_quantity INTEGER DEFAULT 0,
    reserved_quantity INTEGER DEFAULT 0
);

CREATE TABLE repair_service_schema.diagnostics (
    diagnosticid SERIAL PRIMARY KEY,
    orderid INTEGER REFERENCES repair_service_schema.orders(orderid) ON DELETE CASCADE,
    empid INTEGER REFERENCES repair_service_schema.employees(empid),
    deviceid INTEGER REFERENCES repair_service_schema.devices(deviceid),
    diagnostic_date DATE DEFAULT CURRENT_DATE,
    problems TEXT,
    estimated_cost DECIMAL(10, 2),
    requires_prepayment BOOLEAN DEFAULT FALSE
);

CREATE TABLE repair_service_schema.orderparts (
    orderpartid SERIAL PRIMARY KEY,
    orderid INTEGER REFERENCES repair_service_schema.orders(orderid) ON DELETE CASCADE,
    partid INTEGER REFERENCES repair_service_schema.parts(partid),
    quantity INTEGER DEFAULT 1
);

CREATE TABLE repair_service_schema.payments (
    paymentid SERIAL PRIMARY KEY,
    orderid INTEGER REFERENCES repair_service_schema.orders(orderid) ON DELETE CASCADE,
    amount DECIMAL(10, 2) NOT NULL,
    payment_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    payment_type VARCHAR(50)
);

CREATE TABLE repair_service_schema.guarantee (
    guaranteeid SERIAL PRIMARY KEY,
    orderid INTEGER REFERENCES repair_service_schema.orders(orderid) ON DELETE CASCADE,
    problem_description TEXT,
    complaint_date DATE DEFAULT CURRENT_DATE,
    resolution_date DATE,
    compensation_amount DECIMAL(10, 2)
);

CREATE TABLE repair_service_schema.activitylog (
    logid SERIAL PRIMARY KEY,
    orderid INTEGER REFERENCES repair_service_schema.orders(orderid) ON DELETE SET NULL, -- Исправлено: допускает NULL
    empid INTEGER REFERENCES repair_service_schema.employees(empid),
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    action TEXT NOT NULL
);