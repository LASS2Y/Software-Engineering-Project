using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;

namespace KitBox.DataAccess.Repositories;

public class LockerRepository : ILockerRepository
{
    private readonly DatabaseConnection _db;

    public LockerRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public Locker? GetById(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, cabinet_id, height, width, depth, color, has_doors, door_color
              FROM locker WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapLocker(reader);
        }
        return null;
    }

    public List<Locker> GetByCabinetId(int cabinetId)
    {
        var lockers = new List<Locker>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"SELECT id, cabinet_id, height, width, depth, color, has_doors, door_color
              FROM locker WHERE cabinet_id = @cabinetId",
            connection);
        cmd.Parameters.AddWithValue("@cabinetId", cabinetId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lockers.Add(MapLocker(reader));
        }
        return lockers;
    }

    public void Add(Locker locker)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"INSERT INTO locker (cabinet_id, height, width, depth, color, has_doors, door_color)
              VALUES (@cabinetId, @height, @width, @depth, @color, @hasDoors, @doorColor)",
            connection);
        cmd.Parameters.AddWithValue("@cabinetId", locker.CabinetId);
        cmd.Parameters.AddWithValue("@height", locker.Height);
        cmd.Parameters.AddWithValue("@width", locker.Width);
        cmd.Parameters.AddWithValue("@depth", locker.Depth);
        cmd.Parameters.AddWithValue("@color", locker.Color);
        cmd.Parameters.AddWithValue("@hasDoors", locker.HasDoors);
        cmd.Parameters.AddWithValue("@doorColor", locker.DoorColor ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
        locker.Id = (int)cmd.LastInsertedId;
    }

    public void Update(Locker locker)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"UPDATE locker
              SET cabinet_id = @cabinetId, height = @height, width = @width,
                  depth = @depth, color = @color, has_doors = @hasDoors, door_color = @doorColor
              WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", locker.Id);
        cmd.Parameters.AddWithValue("@cabinetId", locker.CabinetId);
        cmd.Parameters.AddWithValue("@height", locker.Height);
        cmd.Parameters.AddWithValue("@width", locker.Width);
        cmd.Parameters.AddWithValue("@depth", locker.Depth);
        cmd.Parameters.AddWithValue("@color", locker.Color);
        cmd.Parameters.AddWithValue("@hasDoors", locker.HasDoors);
        cmd.Parameters.AddWithValue("@doorColor", locker.DoorColor ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand("DELETE FROM locker WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static Locker MapLocker(MySqlDataReader reader)
    {
        return new Locker
        {
            Id = reader.GetInt32("id"),
            CabinetId = reader.GetInt32("cabinet_id"),
            Height = reader.GetDouble("height"),
            Width = reader.GetDouble("width"),
            Depth = reader.GetDouble("depth"),
            Color = reader.GetString("color"),
            HasDoors = reader.GetBoolean("has_doors"),
            DoorColor = reader.IsDBNull(reader.GetOrdinal("door_color")) ? null : reader.GetString("door_color")
        };
    }
}
