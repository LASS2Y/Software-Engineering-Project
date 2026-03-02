using System;
using MySqlConnector;

namespace KitBox.DataAccess;

/// <summary>
/// Manages the connection to the MariaDB database.
/// Provides a centralized way to obtain database connections.
/// </summary>
public class DatabaseConnection
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new DatabaseConnection with the given connection parameters.
    /// </summary>
    public DatabaseConnection(string server, string database, string user, string password, int port = 3306)
    {
        _connectionString = new MySqlConnectionStringBuilder
        {
            Server = server,
            Database = database,
            UserID = user,
            Password = password,
            Port = (uint)port,
            SslMode = MySqlSslMode.Preferred
        }.ConnectionString;
    }

    /// <summary>
    /// Initializes a new DatabaseConnection with a raw connection string.
    /// </summary>
    public DatabaseConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Creates and opens a new connection to the MariaDB database.
    /// The caller is responsible for disposing the connection.
    /// </summary>
    public MySqlConnection GetConnection()
    {
        var connection = new MySqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
