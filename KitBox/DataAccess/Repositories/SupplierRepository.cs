using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;

namespace KitBox.DataAccess.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly DatabaseConnection _db;

    public SupplierRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public Supplier? GetById(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            "SELECT id, name, contact_email, phone FROM supplier WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapSupplier(reader);
        }
        return null;
    }

    public List<Supplier> GetAll()
    {
        var suppliers = new List<Supplier>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            "SELECT id, name, contact_email, phone FROM supplier",
            connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            suppliers.Add(MapSupplier(reader));
        }
        return suppliers;
    }

    public void Add(Supplier supplier)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"INSERT INTO supplier (name, contact_email, phone)
              VALUES (@name, @contactEmail, @phone)",
            connection);
        cmd.Parameters.AddWithValue("@name", supplier.Name);
        cmd.Parameters.AddWithValue("@contactEmail", supplier.ContactEmail);
        cmd.Parameters.AddWithValue("@phone", supplier.Phone ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
        supplier.Id = (int)cmd.LastInsertedId;
    }

    public void Update(Supplier supplier)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"UPDATE supplier SET name = @name, contact_email = @contactEmail, phone = @phone
              WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", supplier.Id);
        cmd.Parameters.AddWithValue("@name", supplier.Name);
        cmd.Parameters.AddWithValue("@contactEmail", supplier.ContactEmail);
        cmd.Parameters.AddWithValue("@phone", supplier.Phone ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand("DELETE FROM supplier WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static Supplier MapSupplier(MySqlDataReader reader)
    {
        return new Supplier
        {
            Id = reader.GetInt32("id"),
            Name = reader.GetString("name"),
            ContactEmail = reader.GetString("contact_email"),
            Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? string.Empty : reader.GetString("phone")
        };
    }
}
