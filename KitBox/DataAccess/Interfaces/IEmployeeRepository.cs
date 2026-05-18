using System.Collections.Generic;
using System.Threading.Tasks;
using KitBox.Models;

namespace KitBox.DataAccess.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee> CreateAsync(Employee employee);
    Task<bool> UpdateAsync(int id, Employee employee);
    Task<bool> DeleteAsync(int id);
    Task<Employee?> GetByEmailAsync(string email);
    bool VerifyPassword(string inputPassword, string storedHash);
}

// Repositories/IEmployeeRepository.cs



  
