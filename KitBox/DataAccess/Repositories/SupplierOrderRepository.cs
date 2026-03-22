using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;

namespace KitBox.DataAccess.Repositories;

public class SupplierOrderRepository : ISupplierOrderRepository
{
    private readonly DatabaseConnection _db;

    public SupplierOrderRepository(DatabaseConnection db)
    {
        _db = db;
        EnsureTable();
    }

    public List<SupplierOrder> GetByCustomerOrderId(int customerOrderId)
    {
        var orders = new List<SupplierOrder>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, customer_order_id, part_id, supplier_id, quantity, unit_cost,
                     delivery_days, ordered_at, expected_delivery_date, status
              FROM supplier_order
              WHERE customer_order_id = @customerOrderId
              ORDER BY id",
            connection);
        cmd.Parameters.AddWithValue("@customerOrderId", customerOrderId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            orders.Add(Map(reader));
        }

        return orders;
    }

    public void Add(SupplierOrder supplierOrder)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"INSERT INTO supplier_order
                (customer_order_id, part_id, supplier_id, quantity, unit_cost,
                 delivery_days, ordered_at, expected_delivery_date, status)
              VALUES
                (@customerOrderId, @partId, @supplierId, @quantity, @unitCost,
                 @deliveryDays, @orderedAt, @expectedDeliveryDate, @status)",
            connection);

        cmd.Parameters.AddWithValue("@customerOrderId", supplierOrder.CustomerOrderId.HasValue
            ? supplierOrder.CustomerOrderId.Value
            : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@partId", supplierOrder.PartId);
        cmd.Parameters.AddWithValue("@supplierId", supplierOrder.SupplierId);
        cmd.Parameters.AddWithValue("@quantity", supplierOrder.Quantity);
        cmd.Parameters.AddWithValue("@unitCost", supplierOrder.UnitCost);
        cmd.Parameters.AddWithValue("@deliveryDays", supplierOrder.DeliveryDays);
        cmd.Parameters.AddWithValue("@orderedAt", supplierOrder.OrderedAt);
        cmd.Parameters.AddWithValue("@expectedDeliveryDate", supplierOrder.ExpectedDeliveryDate);
        cmd.Parameters.AddWithValue("@status", supplierOrder.Status);

        cmd.ExecuteNonQuery();
        supplierOrder.Id = (int)cmd.LastInsertedId;
    }

    private void EnsureTable()
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"CREATE TABLE IF NOT EXISTS supplier_order (
                id                      INT AUTO_INCREMENT PRIMARY KEY,
                customer_order_id       INT NULL,
                part_id                 INT NOT NULL,
                supplier_id             INT NOT NULL,
                quantity                INT NOT NULL,
                unit_cost               DECIMAL(10,4) NOT NULL,
                delivery_days           INT NOT NULL,
                ordered_at              DATE NOT NULL,
                expected_delivery_date  DATE NOT NULL,
                status                  VARCHAR(30) NOT NULL DEFAULT 'Ordered',
                FOREIGN KEY (customer_order_id) REFERENCES customer_order(id) ON DELETE CASCADE,
                FOREIGN KEY (part_id) REFERENCES part(id),
                FOREIGN KEY (supplier_id) REFERENCES supplier(id),
                INDEX idx_supplier_order_customer_order (customer_order_id),
                INDEX idx_supplier_order_expected_delivery (expected_delivery_date)
              ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
            connection);
        cmd.ExecuteNonQuery();

        // Migration safety: allow stock replenishment orders without a customer order link.
        using var alterCmd = new MySqlCommand(
            "ALTER TABLE supplier_order MODIFY customer_order_id INT NULL",
            connection);
        alterCmd.ExecuteNonQuery();
    }

    private static SupplierOrder Map(MySqlDataReader reader)
    {
        return new SupplierOrder
        {
            Id = reader.GetInt32("id"),
            CustomerOrderId = reader.IsDBNull(reader.GetOrdinal("customer_order_id"))
                ? null
                : reader.GetInt32("customer_order_id"),
            PartId = reader.GetInt32("part_id"),
            SupplierId = reader.GetInt32("supplier_id"),
            Quantity = reader.GetInt32("quantity"),
            UnitCost = reader.GetDecimal("unit_cost"),
            DeliveryDays = reader.GetInt32("delivery_days"),
            OrderedAt = reader.GetDateTime("ordered_at"),
            ExpectedDeliveryDate = reader.GetDateTime("expected_delivery_date"),
            Status = reader.GetString("status")
        };
    }
}