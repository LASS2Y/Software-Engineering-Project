
using  System;
using System.ComponentModel.DataAnnotations;

namespace KitBox.Models;

public class Employee
{
    public int EmployeeId { get; set; }
    [Required] public string Firstname { get; set; } 
    [Required] public string Lastname { get; set; } 
    public string? Email { get; set; }
    [Required]
    public string? PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    
    

    public override string ToString()
    {
        return $" Employee : {Firstname} {Lastname}, Email : {Email}, Password : {PasswordHash} \n";
    }
}