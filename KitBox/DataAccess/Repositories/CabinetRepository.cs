using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;

namespace KitBox.DataAccess.Repositories;

public class CabinetRepository : ICabinetRepository
{
    private readonly DatabaseConnection _db;

    public CabinetRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public Cabinet? GetById(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            "SELECT id, order_id, angle_iron_color FROM cabinet WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapCabinet(reader);
        }
        return null;
    }

    public List<Cabinet> GetByOrderId(int orderId)
    {
        var cabinets = new List<Cabinet>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            "SELECT id, order_id, angle_iron_color FROM cabinet WHERE order_id = @orderId",
            connection);
        cmd.Parameters.AddWithValue("@orderId", orderId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            cabinets.Add(MapCabinet(reader));
        }
        return cabinets;
    }

    public void Add(Cabinet cabinet)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"INSERT INTO cabinet (order_id, angle_iron_color)
              VALUES (@orderId, @angleIronColor)",
            connection);
        cmd.Parameters.AddWithValue("@orderId", cabinet.OrderId);
        cmd.Parameters.AddWithValue("@angleIronColor", cabinet.AngleIronColor);
        cmd.ExecuteNonQuery();
        cabinet.Id = (int)cmd.LastInsertedId;
    }

    public void Update(Cabinet cabinet)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"UPDATE cabinet SET order_id = @orderId, angle_iron_color = @angleIronColor
              WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", cabinet.Id);
        cmd.Parameters.AddWithValue("@orderId", cabinet.OrderId);
        cmd.Parameters.AddWithValue("@angleIronColor", cabinet.AngleIronColor);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand("DELETE FROM cabinet WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static Cabinet MapCabinet(MySqlDataReader reader)
    {
        return new Cabinet
        {
            Id = reader.GetInt32("id"),
            OrderId = reader.GetInt32("order_id"),
            AngleIronColor = reader.GetString("angle_iron_color")
        };
    }
}
