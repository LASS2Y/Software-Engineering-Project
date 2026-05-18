using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KitBox.Models;
using KitBox.DataAccess.Interfaces;

namespace KitBox.DataAccess.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {   
        
        private readonly DatabaseConnection _db;

        public EmployeeRepository(DatabaseConnection db)
        {
            _db = db;
        }
        
        public async Task<IEnumerable<Employee>> GetAllAsync()
        {   
            if (_db == null){throw new InvalidOperationException("_dbConnection est NULL ! Injecte DatabaseConnection dans le constructeur de ta classe");}
                
            var employees = new List<Employee>();
            await using var conn =  _db.CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand("SELECT * FROM Employee", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                employees.Add(MapReaderToEmployee(reader));
            }

            return employees;
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            if (_db == null){throw new InvalidOperationException("_dbConnection est NULL ! Injecte DatabaseConnection dans le constructeur de ta classe");}
            await using var conn =  _db.CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand(
                "SELECT * FROM Employee WHERE EmployeeId = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToEmployee(reader);
            }

            return null;
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            if (_db == null){throw new InvalidOperationException("_dbConnection est NULL ! Injecte DatabaseConnection dans le constructeur de ta classe");}
            await using var conn =  _db.CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand(
                "SELECT * FROM Employee WHERE Email = @email", conn);
            cmd.Parameters.AddWithValue("@email", email);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToEmployee(reader);
            }

            return null;
        }

        public async Task<Employee> CreateAsync(Employee employee)
        {
            if (_db == null){throw new InvalidOperationException("_dbConnection est NULL ! Injecte DatabaseConnection dans le constructeur de ta classe");}
            await using var conn =  _db.CreateConnection();
            await conn.OpenAsync();
            
            await using var cmd = new MySqlCommand(@"
                INSERT INTO Employee (FirstName, LastName, Email, PasswordHash, CreatedAt)
                VALUES (@FirstName, @LastName, @Email, @PasswordHash, @CreatedAt);
                SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@FirstName", employee.Firstname);
            cmd.Parameters.AddWithValue("@LastName", employee.Lastname);
            cmd.Parameters.AddWithValue("@Email", (object?)employee.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PasswordHash", (object?)employee.PasswordHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            // Récupérer l'ID généré
  
            try
            {
                var result = await cmd.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                    throw new InvalidOperationException("L'insertion a échoué.");

                employee.EmployeeId = Convert.ToInt32(result);
                employee.PasswordHash = employee.PasswordHash;
                return employee;
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
            {
                throw new InvalidOperationException("Cet email existe déjà.", ex);
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException($"Erreur base de données : {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateAsync(int id, Employee employee)
        {
            if (_db == null){throw new InvalidOperationException("_dbConnection est NULL ! Injecte DatabaseConnection dans le constructeur de ta classe");}
            await using var conn =  _db.CreateConnection();
            await conn.OpenAsync();

            // Vérifier si l'employé existe
            var existing = await GetByIdAsync(id);
            if (existing == null)
                return false;

          

            await using var cmd = new MySqlCommand(@" UPDATE Employee 
                    SET FirstName = @FirstName, 
                        LastName = @LastName, 
                        Email = @Email,
                        PasswordHash = COALESCE(@PasswordHash, PasswordHash)
                    WHERE EmployeeId = @Id", conn);

            cmd.Parameters.AddWithValue("@FirstName", employee.Firstname);
            cmd.Parameters.AddWithValue("@LastName", employee.Lastname);
            cmd.Parameters.AddWithValue("@Email", (object?)employee.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PasswordHash", (object?)employee.PasswordHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Id", id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (_db == null){throw new InvalidOperationException("_dbConnection est NULL ! Injecte DatabaseConnection dans le constructeur de ta classe");}
            await using var conn =  _db.CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand(
                "DELETE FROM Employee WHERE EmployeeId = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        }

        // Méthode utilitaire pour mapper un DataRow vers un Employee
        private static Employee MapReaderToEmployee(MySqlDataReader reader)
        {
            return new Employee
            {
                EmployeeId = reader.GetInt32("EmployeeId"),
                Firstname = reader.GetString("FirstName"),
                Lastname = reader.GetString("LastName"),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) 
                    ? null 
                    : reader.GetString("Email"),
                PasswordHash = reader.IsDBNull(reader.GetOrdinal("PasswordHash")) 
                    ? null 
                    : reader.GetString("PasswordHash"),
                CreatedAt = reader.GetDateTime("CreatedAt")
            };
        }
    }
}              