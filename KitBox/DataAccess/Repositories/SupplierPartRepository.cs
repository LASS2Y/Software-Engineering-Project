using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;

namespace KitBox.DataAccess.Repositories;

/// <summary>
/// Repository for the supplier catalog (supplier ↔ part mapping with price & delivery time).
/// Implements the business rule: best price first, then shortest delivery time.
/// </summary>
public class SupplierPartRepository : ISupplierPartRepository
{
    private readonly DatabaseConnection _db;

    public SupplierPartRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public List<SupplierPart> GetAll()
    {
        var results = new List<SupplierPart>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT sp.id, sp.supplier_id, sp.part_id, sp.price, sp.delivery_days,
                     s.name AS supplier_name, s.contact_email, s.phone,
                     p.name AS part_name, p.reference AS part_reference, p.part_type
              FROM supplier_part sp
              JOIN supplier s ON s.id = sp.supplier_id
              JOIN part     p ON p.id = sp.part_id
              ORDER BY s.name, p.name",
            connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var sp = MapSupplierPart(reader);
            sp.Supplier = new Supplier
            {
                Id           = reader.GetInt32("supplier_id"),
                Name         = reader.GetString("supplier_name"),
                ContactEmail = reader.GetString("contact_email"),
                Phone        = reader.IsDBNull(reader.GetOrdinal("phone")) ? string.Empty : reader.GetString("phone")
            };
            sp.PartName      = reader.GetString("part_name");
            sp.PartReference = reader.GetString("part_reference");
            sp.PartType      = reader.GetString("part_type");
            results.Add(sp);
        }
        return results;
    }

    public List<SupplierPart> GetByPartId(int partId)
    {
        var results = new List<SupplierPart>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, supplier_id, part_id, price, delivery_days
              FROM supplier_part WHERE part_id = @partId
              ORDER BY price ASC, delivery_days ASC",
            connection);
        cmd.Parameters.AddWithValue("@partId", partId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(MapSupplierPart(reader));
        }
        return results;
    }

    public List<SupplierPart> GetBySupplierId(int supplierId)
    {
        var results = new List<SupplierPart>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, supplier_id, part_id, price, delivery_days
              FROM supplier_part WHERE supplier_id = @supplierId",
            connection);
        cmd.Parameters.AddWithValue("@supplierId", supplierId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(MapSupplierPart(reader));
        }
        return results;
    }

    /// <summary>
    /// Returns the best supplier for a given part.
    /// Selection rule: lowest price, then shortest delivery time (as per context.md).
    /// </summary>
    public SupplierPart? GetBestSupplierForPart(int partId)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, supplier_id, part_id, price, delivery_days
              FROM supplier_part
              WHERE part_id = @partId
              ORDER BY price ASC, delivery_days ASC
              LIMIT 1",
            connection);
        cmd.Parameters.AddWithValue("@partId", partId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapSupplierPart(reader);
        }
        return null;
    }

    public void Add(SupplierPart supplierPart)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"INSERT INTO supplier_part (supplier_id, part_id, price, delivery_days)
              VALUES (@supplierId, @partId, @price, @deliveryDays)",
            connection);
        cmd.Parameters.AddWithValue("@supplierId", supplierPart.SupplierId);
        cmd.Parameters.AddWithValue("@partId", supplierPart.PartId);
        cmd.Parameters.AddWithValue("@price", supplierPart.Price);
        cmd.Parameters.AddWithValue("@deliveryDays", supplierPart.DeliveryDays);
        cmd.ExecuteNonQuery();
        supplierPart.Id = (int)cmd.LastInsertedId;
    }

    /// <summary>
    /// Updates price and delivery days (used by secretary to update supplier catalog).
    /// </summary>
    public void Update(SupplierPart supplierPart)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"UPDATE supplier_part SET price = @price, delivery_days = @deliveryDays
              WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", supplierPart.Id);
        cmd.Parameters.AddWithValue("@price", supplierPart.Price);
        cmd.Parameters.AddWithValue("@deliveryDays", supplierPart.DeliveryDays);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand("DELETE FROM supplier_part WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static SupplierPart MapSupplierPart(MySqlDataReader reader)
    {
        return new SupplierPart
        {
            Id = reader.GetInt32("id"),
            SupplierId = reader.GetInt32("supplier_id"),
            PartId = reader.GetInt32("part_id"),
            Price = reader.GetDecimal("price"),
            DeliveryDays = reader.GetInt32("delivery_days")
        };
    }
}
