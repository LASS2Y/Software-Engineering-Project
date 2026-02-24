using System;
using System.Collections.Generic;
using MySqlConnector;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;

namespace KitBox.DataAccess.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly DatabaseConnection _db;

    public CustomerRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public Customer? GetById(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            "SELECT id, first_name, last_name, email, phone FROM customer WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapCustomer(reader);
        }
        return null;
    }

    public List<Customer> GetAll()
    {
        var customers = new List<Customer>();
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            "SELECT id, first_name, last_name, email, phone FROM customer",
            connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            customers.Add(MapCustomer(reader));
        }
        return customers;
    }

    public void Add(Customer customer)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"INSERT INTO customer (first_name, last_name, email, phone)
              VALUES (@firstName, @lastName, @email, @phone)",
            connection);
        cmd.Parameters.AddWithValue("@firstName", customer.FirstName);
        cmd.Parameters.AddWithValue("@lastName", customer.LastName);
        cmd.Parameters.AddWithValue("@email", customer.Email);
        cmd.Parameters.AddWithValue("@phone", customer.Phone ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
        customer.Id = (int)cmd.LastInsertedId;
    }

    public void Update(Customer customer)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand(
            @"UPDATE customer
              SET first_name = @firstName, last_name = @lastName,
                  email = @email, phone = @phone
              WHERE id = @id",
            connection);
        cmd.Parameters.AddWithValue("@id", customer.Id);
        cmd.Parameters.AddWithValue("@firstName", customer.FirstName);
        cmd.Parameters.AddWithValue("@lastName", customer.LastName);
        cmd.Parameters.AddWithValue("@email", customer.Email);
        cmd.Parameters.AddWithValue("@phone", customer.Phone ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = _db.GetConnection();
        using var cmd = new MySqlCommand("DELETE FROM customer WHERE id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static Customer MapCustomer(MySqlDataReader reader)
    {
        return new Customer
        {
            Id = reader.GetInt32("id"),
            FirstName = reader.GetString("first_name"),
            LastName = reader.GetString("last_name"),
            Email = reader.GetString("email"),
            Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? string.Empty : reader.GetString("phone")
        };
    }
}
