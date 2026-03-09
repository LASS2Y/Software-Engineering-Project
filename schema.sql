-- ============================================================
-- KitBox Database Schema (MariaDB)
-- ============================================================

CREATE DATABASE IF NOT EXISTS kitbox;
USE kitbox;

-- Drop tables in reverse dependency order
DROP TABLE IF EXISTS order_line;
DROP TABLE IF EXISTS supplier_part;
DROP TABLE IF EXISTS locker;
DROP TABLE IF EXISTS cabinet;
DROP TABLE IF EXISTS customer_order;
DROP TABLE IF EXISTS bill;
DROP TABLE IF EXISTS customer;
DROP TABLE IF EXISTS part;
DROP TABLE IF EXISTS supplier;

-- ============================================================
-- CUSTOMER
-- ============================================================
CREATE TABLE customer (
    id              INT             AUTO_INCREMENT PRIMARY KEY,
    first_name      VARCHAR(100)    NOT NULL,
    last_name       VARCHAR(100)    NOT NULL,
    email           VARCHAR(255)    NOT NULL UNIQUE,
    phone           VARCHAR(20)     NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- BILL (invoice issued after delivery)
-- ============================================================
CREATE TABLE bill (
    id              INT             AUTO_INCREMENT PRIMARY KEY,
    emission_date   DATE            NOT NULL,
    amount          DECIMAL(10,2)   NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- CUSTOMER_ORDER
-- If parts are not all in stock, the customer pays a deposit
-- and picks up when available. Payment upon receipt → bill.
-- ============================================================
CREATE TABLE customer_order (
    id              INT             AUTO_INCREMENT PRIMARY KEY,
    customer_id     INT             NOT NULL,
    bill_id         INT             NULL,
    order_date      DATE            NOT NULL,
    deposit         DECIMAL(10,2)   NULL,
    available_date  DATE            NULL,
    status          ENUM('Pending','PartiallyAvailable','Available','Delivered','Cancelled')
                                    NOT NULL DEFAULT 'Pending',

    FOREIGN KEY (customer_id) REFERENCES customer(id),
    FOREIGN KEY (bill_id)     REFERENCES bill(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- CABINET (armoire = up to 7 lockers + 4 angle irons)
-- angle_iron_length and locker_count are computed in code.
-- ============================================================
CREATE TABLE cabinet (
    id                  INT             AUTO_INCREMENT PRIMARY KEY,
    order_id            INT             NOT NULL,
    angle_iron_color    VARCHAR(50)     NOT NULL,

    FOREIGN KEY (order_id) REFERENCES customer_order(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- LOCKER (casier inside a cabinet)
-- total_height = height + 2 × 2 cm (computed in code)
-- ============================================================
CREATE TABLE locker (
    id              INT             AUTO_INCREMENT PRIMARY KEY,
    cabinet_id      INT             NOT NULL,
    height          DOUBLE          NOT NULL COMMENT 'Batten height in cm',
    width           DOUBLE          NOT NULL,
    depth           DOUBLE          NOT NULL,
    color           VARCHAR(50)     NOT NULL COMMENT 'Panel color (same for all panels)',
    has_doors       BOOLEAN         NOT NULL DEFAULT FALSE,
    door_color      VARCHAR(50)     NULL COMMENT 'Door color (can differ from panel color)',

    FOREIGN KEY (cabinet_id) REFERENCES cabinet(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- PART (Single Table Inheritance for all part types)
-- part_type discriminator: Panel, Crossbar, Batten, AngleIron, Door, Handle
-- Nullable columns for type-specific attributes.
-- ============================================================
CREATE TABLE part (
    id              INT             AUTO_INCREMENT PRIMARY KEY,
    reference       VARCHAR(50)     NOT NULL UNIQUE,
    name            VARCHAR(255)    NOT NULL,
    part_type       ENUM('Panel','Crossbar','Batten','AngleIron','Door','Handle')
                                    NOT NULL,
    height          DOUBLE          NOT NULL,
    width           DOUBLE          NOT NULL,
    depth           DOUBLE          NOT NULL,
    color           VARCHAR(50)     NOT NULL,
    unit_price      DECIMAL(10,2)   NOT NULL,
    stock_quantity  INT             NOT NULL DEFAULT 0,
    minimum_stock   INT             NOT NULL DEFAULT 0,

    -- Panel-specific
    panel_type      ENUM('Horizontal','Side','Back') NULL,

    -- Crossbar-specific
    crossbar_type   ENUM('Front','Back','Side') NULL,
    groove_count    INT             NULL,

    -- AngleIron-specific
    standard_length DOUBLE          NULL,

    -- Door-specific
    is_glass        BOOLEAN         NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- ORDER_LINE (each part line in an order, with quantity & price)
-- ============================================================
CREATE TABLE order_line (
    id              INT             AUTO_INCREMENT PRIMARY KEY,
    order_id        INT             NOT NULL,
    part_id         INT             NOT NULL,
    quantity        INT             NOT NULL,
    unit_price      DECIMAL(10,2)   NOT NULL COMMENT 'Price at time of order',

    FOREIGN KEY (order_id) REFERENCES customer_order(id) ON DELETE CASCADE,
    FOREIGN KEY (part_id)  REFERENCES part(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- SUPPLIER
-- ============================================================
CREATE TABLE supplier (
    id              INT             AUTO_INCREMENT PRIMARY KEY,
    name            VARCHAR(255)    NOT NULL,
    contact_email   VARCHAR(255)    NOT NULL,
    phone           VARCHAR(20)     NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- SUPPLIER_PART (supplier catalog: N suppliers ↔ N parts)
-- Secretary updates prices regularly from supplier catalogs.
-- Selection rule: lowest price, then shortest delivery time.
-- ============================================================
CREATE TABLE supplier_part (
    id              INT             AUTO_INCREMENT PRIMARY KEY,
    supplier_id     INT             NOT NULL,
    part_id         INT             NOT NULL,
    price           DECIMAL(10,2)   NOT NULL,
    delivery_days   INT             NOT NULL,

    FOREIGN KEY (supplier_id) REFERENCES supplier(id) ON DELETE CASCADE,
    FOREIGN KEY (part_id)     REFERENCES part(id)     ON DELETE CASCADE,
    UNIQUE KEY uk_supplier_part (supplier_id, part_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- Performance indexes
-- ============================================================
CREATE INDEX idx_order_customer      ON customer_order(customer_id);
CREATE INDEX idx_order_status        ON customer_order(status);
CREATE INDEX idx_cabinet_order       ON cabinet(order_id);
CREATE INDEX idx_locker_cabinet      ON locker(cabinet_id);
CREATE INDEX idx_orderline_order     ON order_line(order_id);
CREATE INDEX idx_orderline_part      ON order_line(part_id);
CREATE INDEX idx_part_type           ON part(part_type);
CREATE INDEX idx_part_stock          ON part(stock_quantity, minimum_stock);
CREATE INDEX idx_supplierpart_part   ON supplier_part(part_id);
CREATE INDEX idx_supplierpart_price  ON supplier_part(price, delivery_days);
