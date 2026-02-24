using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.DataAccess.Interfaces;
using KitBox.Models.Parts;
using KitBox.Models.Enums;

namespace KitBox.DataAccess.Repositories;

/// <summary>
/// Repository for Part using Single Table Inheritance.
/// Maps the part_type discriminator column to the correct C# subclass.
/// </summary>
public class PartRepository : IPartRepository
{
    private readonly DatabaseConnection _db;

    public PartRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public Part? GetById(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, reference, name, part_type, height, width, depth, color,
                     unit_price, stock_quantity, minimum_stock,
                     panel_type, crossbar_type, groove_count, standard_length, is_glass
              FROM part WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapPart(reader);
        }
        return null;
    }

    public Part? GetByReference(string reference)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, reference, name, part_type, height, width, depth, color,
                     unit_price, stock_quantity, minimum_stock,
                     panel_type, crossbar_type, groove_count, standard_length, is_glass
              FROM part WHERE reference = @reference",
            connection);
        cmd.Parameters.AddWithValue("@reference", reference);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapPart(reader);
        }
        return null;
    }

    public List<Part> GetAll()
    {
        var parts = new List<Part>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, reference, name, part_type, height, width, depth, color,
                     unit_price, stock_quantity, minimum_stock,
                     panel_type, crossbar_type, groove_count, standard_length, is_glass
              FROM part",
            connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            parts.Add(MapPart(reader));
        }
        return parts;
    }

    public List<Part> GetByType(string partType)
    {
        var parts = new List<Part>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, reference, name, part_type, height, width, depth, color,
                     unit_price, stock_quantity, minimum_stock,
                     panel_type, crossbar_type, groove_count, standard_length, is_glass
              FROM part WHERE part_type = @partType",
            connection);
        cmd.Parameters.AddWithValue("@partType", partType);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            parts.Add(MapPart(reader));
        }
        return parts;
    }

    /// <summary>
    /// Returns all parts where stock_quantity is below minimum_stock.
    /// </summary>
    public List<Part> GetLowStock()
    {
        var parts = new List<Part>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, reference, name, part_type, height, width, depth, color,
                     unit_price, stock_quantity, minimum_stock,
                     panel_type, crossbar_type, groove_count, standard_length, is_glass
              FROM part WHERE stock_quantity < minimum_stock",
            connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            parts.Add(MapPart(reader));
        }
        return parts;
    }

    public void Add(Part part)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"INSERT INTO part (reference, name, part_type, height, width, depth, color,
                                unit_price, stock_quantity, minimum_stock,
                                panel_type, crossbar_type, groove_count, standard_length, is_glass)
              VALUES (@reference, @name, @partType, @height, @width, @depth, @color,
                      @unitPrice, @stockQuantity, @minimumStock,
                      @panelType, @crossbarType, @grooveCount, @standardLength, @isGlass)",
            connection);

        AddCommonParameters(cmd, part);
        AddTypeSpecificParameters(cmd, part);
        cmd.ExecuteNonQuery();
        part.Id = (int)cmd.LastInsertedId;
    }

    public void Update(Part part)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"UPDATE part SET
                reference = @reference, name = @name, part_type = @partType,
                height = @height, width = @width, depth = @depth, color = @color,
                unit_price = @unitPrice, stock_quantity = @stockQuantity,
                minimum_stock = @minimumStock,
                panel_type = @panelType, crossbar_type = @crossbarType,
                groove_count = @grooveCount, standard_length = @standardLength,
                is_glass = @isGlass
              WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", part.Id);
        AddCommonParameters(cmd, part);
        AddTypeSpecificParameters(cmd, part);
        cmd.ExecuteNonQuery();
    }

    public void UpdateStock(int id, int quantity)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            "UPDATE part SET stock_quantity = @quantity WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@quantity", quantity);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand("DELETE FROM part WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ──────── Private helpers ────────

    private static void AddCommonParameters(MySqlCommand cmd, Part part)
    {
        string partType = part switch
        {
            Panel => "Panel",
            Crossbar => "Crossbar",
            Batten => "Batten",
            AngleIron => "AngleIron",
            Door => "Door",
            Handle => "Handle",
            _ => throw new ArgumentException($"Unknown part type: {part.GetType().Name}")
        };

        cmd.Parameters.AddWithValue("@reference", part.Reference);
        cmd.Parameters.AddWithValue("@name", part.Name);
        cmd.Parameters.AddWithValue("@partType", partType);
        cmd.Parameters.AddWithValue("@height", part.Height);
        cmd.Parameters.AddWithValue("@width", part.Width);
        cmd.Parameters.AddWithValue("@depth", part.Depth);
        cmd.Parameters.AddWithValue("@color", part.Color);
        cmd.Parameters.AddWithValue("@unitPrice", part.UnitPrice);
        cmd.Parameters.AddWithValue("@stockQuantity", part.StockQuantity);
        cmd.Parameters.AddWithValue("@minimumStock", part.MinimumStock);
    }

    private static void AddTypeSpecificParameters(MySqlCommand cmd, Part part)
    {
        // Panel
        cmd.Parameters.AddWithValue("@panelType",
            part is Panel panel ? panel.Type.ToString() : (object)DBNull.Value);

        // Crossbar
        cmd.Parameters.AddWithValue("@crossbarType",
            part is Crossbar crossbar ? crossbar.Type.ToString() : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@grooveCount",
            part is Crossbar cb ? cb.GrooveCount : (object)DBNull.Value);

        // AngleIron
        cmd.Parameters.AddWithValue("@standardLength",
            part is AngleIron angleIron ? angleIron.StandardLength : (object)DBNull.Value);

        // Door
        cmd.Parameters.AddWithValue("@isGlass",
            part is Door door ? door.IsGlass : (object)DBNull.Value);
    }

    /// <summary>
    /// Maps a database row to the correct Part subclass based on part_type discriminator.
    /// </summary>
    private static Part MapPart(MySqlDataReader reader)
    {
        string partType = reader.GetString("part_type");
        Part part = partType switch
        {
            "Panel" => new Panel
            {
                Type = Enum.Parse<PanelType>(reader.GetString("panel_type"))
            },
            "Crossbar" => new Crossbar
            {
                Type = Enum.Parse<CrossbarType>(reader.GetString("crossbar_type")),
                GrooveCount = reader.GetInt32("groove_count")
            },
            "Batten" => new Batten(),
            "AngleIron" => new AngleIron
            {
                StandardLength = reader.GetDouble("standard_length")
            },
            "Door" => new Door
            {
                IsGlass = reader.GetBoolean("is_glass")
            },
            "Handle" => new Handle(),
            _ => throw new InvalidOperationException($"Unknown part_type: {partType}")
        };

        // Map common fields
        part.Id = reader.GetInt32("id");
        part.Reference = reader.GetString("reference");
        part.Name = reader.GetString("name");
        part.Height = reader.GetDouble("height");
        part.Width = reader.GetDouble("width");
        part.Depth = reader.GetDouble("depth");
        part.Color = reader.GetString("color");
        part.UnitPrice = reader.GetDecimal("unit_price");
        part.StockQuantity = reader.GetInt32("stock_quantity");
        part.MinimumStock = reader.GetInt32("minimum_stock");

        return part;
    }
}
