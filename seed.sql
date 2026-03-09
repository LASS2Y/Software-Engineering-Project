-- ============================================================
-- KitBox – Seed Data
-- Run after schema.sql:
--   mysql -u root -p kitbox < seed.sql
-- ============================================================

USE kitbox;

-- ============================================================
-- SUPPLIERS
-- ============================================================
INSERT INTO supplier (name, contact_email, phone) VALUES
    ('PanelPro NV',        'orders@panelpro.be',    '+32 2 123 45 67'),
    ('SteelFix Europe',    'supply@steelfix.eu',    '+32 3 987 65 43'),
    ('KitParts Wholesale', 'catalog@kitparts.com',  '+32 9 555 12 34');

-- ============================================================
-- PARTS
-- Dimensions in cm. height/width/depth = 0 when not applicable.
-- ============================================================

-- ── BATTENS (montants verticaux) ─────────────────────────────
-- height = locker height; width/depth not relevant → 0
INSERT INTO part (reference, name, part_type, height, width, depth, color, unit_price, stock_quantity, minimum_stock) VALUES
    ('BAT-W-25', 'Batten 25cm White',  'Batten', 25, 0, 0, 'White',  3.50, 50, 10),
    ('BAT-W-30', 'Batten 30cm White',  'Batten', 30, 0, 0, 'White',  4.00, 60, 10),
    ('BAT-W-35', 'Batten 35cm White',  'Batten', 35, 0, 0, 'White',  4.50, 40, 10),
    ('BAT-W-40', 'Batten 40cm White',  'Batten', 40, 0, 0, 'White',  5.00, 35, 10),
    ('BAT-W-50', 'Batten 50cm White',  'Batten', 50, 0, 0, 'White',  6.00, 30, 10),
    ('BAT-B-25', 'Batten 25cm Black',  'Batten', 25, 0, 0, 'Black',  3.50, 30, 5),
    ('BAT-B-30', 'Batten 30cm Black',  'Batten', 30, 0, 0, 'Black',  4.00, 30, 5),
    ('BAT-B-40', 'Batten 40cm Black',  'Batten', 40, 0, 0, 'Black',  5.00, 20, 5),
    ('BAT-G-30', 'Batten 30cm Grey',   'Batten', 30, 0, 0, 'Grey',   4.00, 25, 5),
    ('BAT-O-30', 'Batten 30cm Oak',    'Batten', 30, 0, 0, 'Oak',    5.50, 15, 5);

-- ── CROSSBARS – Front (2 grooves, placed at top/bottom of locker front) ──
-- width = locker width
INSERT INTO part (reference, name, part_type, height, width, depth, color, unit_price, stock_quantity, minimum_stock, crossbar_type, groove_count) VALUES
    ('CRF-W-40', 'Front Crossbar 40cm White', 'Crossbar', 0, 40, 0, 'White', 2.80, 40, 8, 'Front', 2),
    ('CRF-W-60', 'Front Crossbar 60cm White', 'Crossbar', 0, 60, 0, 'White', 3.50, 40, 8, 'Front', 2),
    ('CRF-W-80', 'Front Crossbar 80cm White', 'Crossbar', 0, 80, 0, 'White', 4.20, 30, 8, 'Front', 2),
    ('CRF-W-100','Front Crossbar 100cm White','Crossbar', 0,100, 0, 'White', 5.00, 20, 5, 'Front', 2),
    ('CRF-B-40', 'Front Crossbar 40cm Black', 'Crossbar', 0, 40, 0, 'Black', 2.80, 20, 5, 'Front', 2),
    ('CRF-B-60', 'Front Crossbar 60cm Black', 'Crossbar', 0, 60, 0, 'Black', 3.50, 20, 5, 'Front', 2),
    ('CRF-B-80', 'Front Crossbar 80cm Black', 'Crossbar', 0, 80, 0, 'Black', 4.20, 15, 5, 'Front', 2),
    ('CRF-G-60', 'Front Crossbar 60cm Grey',  'Crossbar', 0, 60, 0, 'Grey',  3.50, 15, 5, 'Front', 2),
    ('CRF-O-60', 'Front Crossbar 60cm Oak',   'Crossbar', 0, 60, 0, 'Oak',   4.50, 10, 5, 'Front', 2);

-- ── CROSSBARS – Back (1 groove) ──────────────────────────────
INSERT INTO part (reference, name, part_type, height, width, depth, color, unit_price, stock_quantity, minimum_stock, crossbar_type, groove_count) VALUES
    ('CRB-W-40', 'Back Crossbar 40cm White',  'Crossbar', 0, 40, 0, 'White', 2.60, 40, 8, 'Back', 1),
    ('CRB-W-60', 'Back Crossbar 60cm White',  'Crossbar', 0, 60, 0, 'White', 3.20, 40, 8, 'Back', 1),
    ('CRB-W-80', 'Back Crossbar 80cm White',  'Crossbar', 0, 80, 0, 'White', 3.90, 30, 8, 'Back', 1),
    ('CRB-W-100','Back Crossbar 100cm White', 'Crossbar', 0,100, 0, 'White', 4.70, 20, 5, 'Back', 1),
    ('CRB-B-40', 'Back Crossbar 40cm Black',  'Crossbar', 0, 40, 0, 'Black', 2.60, 20, 5, 'Back', 1),
    ('CRB-B-60', 'Back Crossbar 60cm Black',  'Crossbar', 0, 60, 0, 'Black', 3.20, 20, 5, 'Back', 1),
    ('CRB-G-60', 'Back Crossbar 60cm Grey',   'Crossbar', 0, 60, 0, 'Grey',  3.20, 15, 5, 'Back', 1),
    ('CRB-O-60', 'Back Crossbar 60cm Oak',    'Crossbar', 0, 60, 0, 'Oak',   4.20, 10, 5, 'Back', 1);

-- ── CROSSBARS – Side (1 groove, width = locker depth) ────────
INSERT INTO part (reference, name, part_type, height, width, depth, color, unit_price, stock_quantity, minimum_stock, crossbar_type, groove_count) VALUES
    ('CRS-W-30', 'Side Crossbar 30cm White',  'Crossbar', 0, 30, 0, 'White', 2.20, 50, 10, 'Side', 1),
    ('CRS-W-40', 'Side Crossbar 40cm White',  'Crossbar', 0, 40, 0, 'White', 2.60, 50, 10, 'Side', 1),
    ('CRS-W-50', 'Side Crossbar 50cm White',  'Crossbar', 0, 50, 0, 'White', 3.00, 30, 10, 'Side', 1),
    ('CRS-B-30', 'Side Crossbar 30cm Black',  'Crossbar', 0, 30, 0, 'Black', 2.20, 25, 5,  'Side', 1),
    ('CRS-B-40', 'Side Crossbar 40cm Black',  'Crossbar', 0, 40, 0, 'Black', 2.60, 25, 5,  'Side', 1),
    ('CRS-G-40', 'Side Crossbar 40cm Grey',   'Crossbar', 0, 40, 0, 'Grey',  2.60, 15, 5,  'Side', 1),
    ('CRS-O-40', 'Side Crossbar 40cm Oak',    'Crossbar', 0, 40, 0, 'Oak',   3.40, 10, 5,  'Side', 1);

-- ── PANELS – Horizontal (top/bottom: width × depth) ──────────
INSERT INTO part (reference, name, part_type, height, width, depth, color, unit_price, stock_quantity, minimum_stock, panel_type) VALUES
    ('PNH-W-4030','Horiz Panel 40×30 White',  'Panel', 0, 40, 30, 'White', 8.00, 30, 6, 'Horizontal'),
    ('PNH-W-4040','Horiz Panel 40×40 White',  'Panel', 0, 40, 40, 'White', 9.50, 30, 6, 'Horizontal'),
    ('PNH-W-4050','Horiz Panel 40×50 White',  'Panel', 0, 40, 50, 'White',11.00, 20, 6, 'Horizontal'),
    ('PNH-W-6030','Horiz Panel 60×30 White',  'Panel', 0, 60, 30, 'White',11.00, 30, 6, 'Horizontal'),
    ('PNH-W-6040','Horiz Panel 60×40 White',  'Panel', 0, 60, 40, 'White',13.00, 30, 6, 'Horizontal'),
    ('PNH-W-6050','Horiz Panel 60×50 White',  'Panel', 0, 60, 50, 'White',15.00, 20, 6, 'Horizontal'),
    ('PNH-W-8030','Horiz Panel 80×30 White',  'Panel', 0, 80, 30, 'White',14.00, 20, 5, 'Horizontal'),
    ('PNH-W-8040','Horiz Panel 80×40 White',  'Panel', 0, 80, 40, 'White',16.50, 20, 5, 'Horizontal'),
    ('PNH-W-8050','Horiz Panel 80×50 White',  'Panel', 0, 80, 50, 'White',19.00, 15, 5, 'Horizontal'),
    ('PNH-W-10040','Horiz Panel 100×40 White','Panel', 0,100, 40, 'White',20.00, 15, 5, 'Horizontal'),
    ('PNH-B-6040','Horiz Panel 60×40 Black',  'Panel', 0, 60, 40, 'Black',13.00, 15, 4, 'Horizontal'),
    ('PNH-G-6040','Horiz Panel 60×40 Grey',   'Panel', 0, 60, 40, 'Grey', 13.00, 10, 4, 'Horizontal'),
    ('PNH-O-6040','Horiz Panel 60×40 Oak',    'Panel', 0, 60, 40, 'Oak',  18.00,  8, 4, 'Horizontal');

-- ── PANELS – Side (height × depth) ───────────────────────────
INSERT INTO part (reference, name, part_type, height, width, depth, color, unit_price, stock_quantity, minimum_stock, panel_type) VALUES
    ('PNS-W-2530','Side Panel 25×30 White',  'Panel', 25, 0, 30, 'White', 6.50, 25, 5, 'Side'),
    ('PNS-W-2540','Side Panel 25×40 White',  'Panel', 25, 0, 40, 'White', 7.50, 25, 5, 'Side'),
    ('PNS-W-3030','Side Panel 30×30 White',  'Panel', 30, 0, 30, 'White', 7.50, 30, 6, 'Side'),
    ('PNS-W-3040','Side Panel 30×40 White',  'Panel', 30, 0, 40, 'White', 8.50, 30, 6, 'Side'),
    ('PNS-W-3050','Side Panel 30×50 White',  'Panel', 30, 0, 50, 'White',10.00, 20, 6, 'Side'),
    ('PNS-W-3530','Side Panel 35×30 White',  'Panel', 35, 0, 30, 'White', 8.50, 20, 5, 'Side'),
    ('PNS-W-3540','Side Panel 35×40 White',  'Panel', 35, 0, 40, 'White', 9.50, 20, 5, 'Side'),
    ('PNS-W-4030','Side Panel 40×30 White',  'Panel', 40, 0, 30, 'White', 9.50, 20, 5, 'Side'),
    ('PNS-W-4040','Side Panel 40×40 White',  'Panel', 40, 0, 40, 'White',11.00, 20, 5, 'Side'),
    ('PNS-W-5040','Side Panel 50×40 White',  'Panel', 50, 0, 40, 'White',13.00, 15, 5, 'Side'),
    ('PNS-B-3040','Side Panel 30×40 Black',  'Panel', 30, 0, 40, 'Black', 8.50, 15, 4, 'Side'),
    ('PNS-G-3040','Side Panel 30×40 Grey',   'Panel', 30, 0, 40, 'Grey',  8.50, 10, 4, 'Side'),
    ('PNS-O-3040','Side Panel 30×40 Oak',    'Panel', 30, 0, 40, 'Oak',  12.00,  8, 4, 'Side');

-- ── PANELS – Back (height × width) ───────────────────────────
INSERT INTO part (reference, name, part_type, height, width, depth, color, unit_price, stock_quantity, minimum_stock, panel_type) VALUES
    ('PNB-W-2540','Back Panel 25×40 White',  'Panel', 25, 40, 0, 'White', 7.00, 20, 5, 'Back'),
    ('PNB-W-2560','Back Panel 25×60 White',  'Panel', 25, 60, 0, 'White', 9.50, 20, 5, 'Back'),
    ('PNB-W-3040','Back Panel 30×40 White',  'Panel', 30, 40, 0, 'White', 8.00, 25, 5, 'Back'),
    ('PNB-W-3060','Back Panel 30×60 White',  'Panel', 30, 60, 0, 'White',11.00, 25, 5, 'Back'),
    ('PNB-W-3080','Back Panel 30×80 White',  'Panel', 30, 80, 0, 'White',14.00, 20, 5, 'Back'),
    ('PNB-W-35100','Back Panel 35×100 White','Panel', 35,100, 0, 'White',18.00, 10, 4, 'Back'),
    ('PNB-W-4060','Back Panel 40×60 White',  'Panel', 40, 60, 0, 'White',14.00, 20, 5, 'Back'),
    ('PNB-W-4080','Back Panel 40×80 White',  'Panel', 40, 80, 0, 'White',17.00, 15, 5, 'Back'),
    ('PNB-W-5060','Back Panel 50×60 White',  'Panel', 50, 60, 0, 'White',17.00, 15, 5, 'Back'),
    ('PNB-W-5080','Back Panel 50×80 White',  'Panel', 50, 80, 0, 'White',21.00, 10, 4, 'Back'),
    ('PNB-B-3060','Back Panel 30×60 Black',  'Panel', 30, 60, 0, 'Black',11.00, 12, 4, 'Back'),
    ('PNB-G-3060','Back Panel 30×60 Grey',   'Panel', 30, 60, 0, 'Grey', 11.00,  8, 4, 'Back'),
    ('PNB-O-3060','Back Panel 30×60 Oak',    'Panel', 30, 60, 0, 'Oak',  16.00,  6, 4, 'Back');

-- ── ANGLE IRONS ───────────────────────────────────────────────
-- standard_length = the catalogued bar length in cm.
-- height column stores the standard length for matching purposes.
INSERT INTO part (reference, name, part_type, height, width, depth, color, unit_price, stock_quantity, minimum_stock, standard_length) VALUES
    ('ANG-W-34',  'Angle Iron 34cm White',   'AngleIron', 34,  0, 0, 'White',  4.50, 40, 8, 34),
    ('ANG-W-38',  'Angle Iron 38cm White',   'AngleIron', 38,  0, 0, 'White',  5.00, 35, 8, 38),
    ('ANG-W-68',  'Angle Iron 68cm White',   'AngleIron', 68,  0, 0, 'White',  8.50, 25, 6, 68),
    ('ANG-W-72',  'Angle Iron 72cm White',   'AngleIron', 72,  0, 0, 'White',  9.00, 20, 6, 72),
    ('ANG-W-102', 'Angle Iron 102cm White',  'AngleIron',102,  0, 0, 'White', 12.50, 15, 5,102),
    ('ANG-W-136', 'Angle Iron 136cm White',  'AngleIron',136,  0, 0, 'White', 16.00, 10, 4,136),
    ('ANG-B-34',  'Angle Iron 34cm Black',   'AngleIron', 34,  0, 0, 'Black',  4.50, 20, 5, 34),
    ('ANG-B-68',  'Angle Iron 68cm Black',   'AngleIron', 68,  0, 0, 'Black',  8.50, 15, 5, 68),
    ('ANG-G-34',  'Angle Iron 34cm Grey',    'AngleIron', 34,  0, 0, 'Grey',   4.50, 15, 5, 34),
    ('ANG-G-68',  'Angle Iron 68cm Grey',    'AngleIron', 68,  0, 0, 'Grey',   8.50, 10, 4, 68);

-- ── DOORS (width = half the locker width) ────────────────────
INSERT INTO part (reference, name, part_type, height, width, depth, color, unit_price, stock_quantity, minimum_stock, is_glass) VALUES
    ('DOR-W-2520','Door 25×20cm White',  'Door', 25, 20, 0, 'White', 12.00, 20, 4, FALSE),
    ('DOR-W-2530','Door 25×30cm White',  'Door', 25, 30, 0, 'White', 14.00, 20, 4, FALSE),
    ('DOR-W-2540','Door 25×40cm White',  'Door', 25, 40, 0, 'White', 16.00, 15, 4, FALSE),
    ('DOR-W-3020','Door 30×20cm White',  'Door', 30, 20, 0, 'White', 13.00, 20, 4, FALSE),
    ('DOR-W-3030','Door 30×30cm White',  'Door', 30, 30, 0, 'White', 15.50, 20, 4, FALSE),
    ('DOR-W-3040','Door 30×40cm White',  'Door', 30, 40, 0, 'White', 18.00, 15, 4, FALSE),
    ('DOR-W-4030','Door 40×30cm White',  'Door', 40, 30, 0, 'White', 18.00, 15, 4, FALSE),
    ('DOR-W-4040','Door 40×40cm White',  'Door', 40, 40, 0, 'White', 21.00, 12, 4, FALSE),
    ('DOR-W-5030','Door 50×30cm White',  'Door', 50, 30, 0, 'White', 22.00, 10, 4, FALSE),
    ('DOR-B-3030','Door 30×30cm Black',  'Door', 30, 30, 0, 'Black', 15.50, 10, 3, FALSE),
    ('DOR-B-3040','Door 30×40cm Black',  'Door', 30, 40, 0, 'Black', 18.00, 10, 3, FALSE),
    ('DOR-GL-3030','Door 30×30 Glass',   'Door', 30, 30, 0, 'White', 24.00,  8, 3, TRUE),
    ('DOR-GL-3040','Door 30×40 Glass',   'Door', 30, 40, 0, 'White', 28.00,  6, 3, TRUE);

-- ── HANDLES ───────────────────────────────────────────────────
INSERT INTO part (reference, name, part_type, height, width, depth, color, unit_price, stock_quantity, minimum_stock) VALUES
    ('HDL-W', 'Cup Handle White', 'Handle', 0, 0, 0, 'White',  1.80, 80, 20),
    ('HDL-B', 'Cup Handle Black', 'Handle', 0, 0, 0, 'Black',  1.80, 50, 15),
    ('HDL-G', 'Cup Handle Grey',  'Handle', 0, 0, 0, 'Grey',   1.80, 30, 10),
    ('HDL-C', 'Cup Handle Chrome','Handle', 0, 0, 0, 'Beige',  2.20, 25, 10);

-- ============================================================
-- SUPPLIER_PART  (who sells what, at what price, how fast)
-- ============================================================

-- PanelPro NV (supplier 1) – sells panels and battens
INSERT INTO supplier_part (supplier_id, part_id, price, delivery_days)
SELECT 1, id, unit_price * 0.85, 3 FROM part WHERE part_type IN ('Panel','Batten');

-- SteelFix Europe (supplier 2) – sells crossbars, angle irons, handles
INSERT INTO supplier_part (supplier_id, part_id, price, delivery_days)
SELECT 2, id, unit_price * 0.82, 5 FROM part WHERE part_type IN ('Crossbar','AngleIron','Handle');

-- KitParts Wholesale (supplier 3) – sells everything but slower
INSERT INTO supplier_part (supplier_id, part_id, price, delivery_days)
SELECT 3, id, unit_price * 0.90, 7 FROM part;

-- SteelFix also sells doors at a competitive price
INSERT INTO supplier_part (supplier_id, part_id, price, delivery_days)
SELECT 2, id, unit_price * 0.88, 4 FROM part WHERE part_type = 'Door';

