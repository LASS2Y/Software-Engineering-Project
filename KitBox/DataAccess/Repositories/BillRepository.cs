using System;
using MySqlConnector;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;

namespace KitBox.DataAccess.Repositories;

public class BillRepository : IBillRepository
{
    private readonly DatabaseConnection _db;

    public BillRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public Bill? GetById(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            "SELECT id, emission_date, amount FROM bill WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Bill
            {
                Id = reader.GetInt32("id"),
                EmissionDate = reader.GetDateTime("emission_date"),
                Amount = reader.GetDecimal("amount")
            };
        }
        return null;
    }

    public void Add(Bill bill)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"INSERT INTO bill (emission_date, amount) VALUES (@emissionDate, @amount)",
            connection);
        cmd.Parameters.AddWithValue("@emissionDate", bill.EmissionDate);
        cmd.Parameters.AddWithValue("@amount", bill.Amount);
        cmd.ExecuteNonQuery();
        bill.Id = (int)cmd.LastInsertedId;
    }
}
