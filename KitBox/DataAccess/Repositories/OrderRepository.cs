using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;
using KitBox.Models.Enums;

namespace KitBox.DataAccess.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly DatabaseConnection _db;

    public OrderRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public Order? GetById(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, customer_id, bill_id, order_date, deposit, available_date, status
              FROM customer_order WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapOrder(reader);
        }
        return null;
    }

    public List<Order> GetAll()
    {
        var orders = new List<Order>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, customer_id, bill_id, order_date, deposit, available_date, status
              FROM customer_order",
            connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            orders.Add(MapOrder(reader));
        }
        return orders;
    }

    public List<Order> GetByCustomerId(int customerId)
    {
        var orders = new List<Order>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, customer_id, bill_id, order_date, deposit, available_date, status
              FROM customer_order WHERE customer_id = @customerId",
            connection);
        cmd.Parameters.AddWithValue("@customerId", customerId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            orders.Add(MapOrder(reader));
        }
        return orders;
    }

    public void Add(Order order)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"INSERT INTO customer_order (customer_id, bill_id, order_date, deposit, available_date, status)
              VALUES (@customerId, @billId, @orderDate, @deposit, @availableDate, @status)",
            connection);
        cmd.Parameters.AddWithValue("@customerId", order.CustomerId);
        cmd.Parameters.AddWithValue("@billId", order.BillId.HasValue ? order.BillId.Value : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@orderDate", order.OrderDate);
        cmd.Parameters.AddWithValue("@deposit", order.Deposit.HasValue ? order.Deposit.Value : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@availableDate", order.AvailableDate.HasValue ? order.AvailableDate.Value : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@status", order.Status.ToString());
        cmd.ExecuteNonQuery();
        order.Id = (int)cmd.LastInsertedId;
    }

    public void Update(Order order)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"UPDATE customer_order
              SET customer_id = @customerId, bill_id = @billId, order_date = @orderDate,
                  deposit = @deposit, available_date = @availableDate, status = @status
              WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", order.Id);
        cmd.Parameters.AddWithValue("@customerId", order.CustomerId);
        cmd.Parameters.AddWithValue("@billId", order.BillId.HasValue ? order.BillId.Value : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@orderDate", order.OrderDate);
        cmd.Parameters.AddWithValue("@deposit", order.Deposit.HasValue ? order.Deposit.Value : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@availableDate", order.AvailableDate.HasValue ? order.AvailableDate.Value : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@status", order.Status.ToString());
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand("DELETE FROM customer_order WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static Order MapOrder(MySqlDataReader reader)
    {
        return new Order
        {
            Id = reader.GetInt32("id"),
            CustomerId = reader.GetInt32("customer_id"),
            BillId = reader.IsDBNull(reader.GetOrdinal("bill_id")) ? null : reader.GetInt32("bill_id"),
            OrderDate = reader.GetDateTime("order_date"),
            Deposit = reader.IsDBNull(reader.GetOrdinal("deposit")) ? null : reader.GetDecimal("deposit"),
            AvailableDate = reader.IsDBNull(reader.GetOrdinal("available_date")) ? null : reader.GetDateTime("available_date"),
            Status = Enum.Parse<OrderStatus>(reader.GetString("status"))
        };
    }
}
