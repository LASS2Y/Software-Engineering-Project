using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;

namespace KitBox.DataAccess.Repositories;

public class OrderLineRepository : IOrderLineRepository
{
    private readonly DatabaseConnection _db;

    public OrderLineRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public List<OrderLine> GetByOrderId(int orderId)
    {
        var lines = new List<OrderLine>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            "SELECT id, order_id, part_id, quantity, unit_price FROM order_line WHERE order_id = @orderId",
            connection);
        cmd.Parameters.AddWithValue("@orderId", orderId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lines.Add(new OrderLine
            {
                Id = reader.GetInt32("id"),
                OrderId = reader.GetInt32("order_id"),
                PartId = reader.GetInt32("part_id"),
                Quantity = reader.GetInt32("quantity"),
                UnitPrice = reader.GetDecimal("unit_price")
            });
        }
        return lines;
    }

    public int GetSoldQuantityByPartSince(int partId, DateTime startDate)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT COALESCE(SUM(ol.quantity), 0)
              FROM order_line ol
              JOIN customer_order o ON o.id = ol.order_id
              WHERE ol.part_id = @partId
                AND o.order_date >= @startDate",
            connection);
        cmd.Parameters.AddWithValue("@partId", partId);
        cmd.Parameters.AddWithValue("@startDate", startDate.Date);

        var result = cmd.ExecuteScalar();
        return Convert.ToInt32(result);
    }

    public void Add(OrderLine orderLine)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"INSERT INTO order_line (order_id, part_id, quantity, unit_price)
              VALUES (@orderId, @partId, @quantity, @unitPrice)",
            connection);
        cmd.Parameters.AddWithValue("@orderId", orderLine.OrderId);
        cmd.Parameters.AddWithValue("@partId", orderLine.PartId);
        cmd.Parameters.AddWithValue("@quantity", orderLine.Quantity);
        cmd.Parameters.AddWithValue("@unitPrice", orderLine.UnitPrice);
        cmd.ExecuteNonQuery();
        orderLine.Id = (int)cmd.LastInsertedId;
    }

    public void Delete(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand("DELETE FROM order_line WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}
