using System;
using System.Threading.Tasks;
using BCrypt.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KitBox.DataAccess;
using KitBox.DataAccess.Interfaces;
using KitBox.DataAccess.Repositories;
using  KitBox.Models;

namespace KitBox.ViewModels;

public partial class EmployeeInscriptionViewModel: ViewModelBase
{
    private readonly MainViewModel _main;
    private readonly Employee _employee;
    private readonly DatabaseConnection _db;
    private readonly IEmployeeRepository _employeeRepository;
    public EmployeeInscriptionViewModel(MainViewModel main)
    {
        _main = main;
        _employee = new Employee();
        var db = new DatabaseConnection(
            server:   EnvConfig.Get("DB_SERVER",   "localhost"),
            database: EnvConfig.Get("DB_NAME",     "kitbox"),
            user:     EnvConfig.Get("DB_USER",     "root"),
            password: EnvConfig.Get("DB_PASSWORD", ""),
            port:     int.Parse(EnvConfig.Get("DB_PORT", "3306")));
        _db = db;
        _employeeRepository = new EmployeeRepository(_db);
    }

    [ObservableProperty] public string _firstName;
    [ObservableProperty] public string _lastName;
    [ObservableProperty] public string _email;
    [ObservableProperty] public string _password;
    [ObservableProperty]  public string _confirmPassword;
    [ObservableProperty] public string _errorMessage; 
    
   

    
    [RelayCommand]
    public void GotoSecretaryMenu()
    {
        _main.GoToSecretaryMenu();
    }

    [RelayCommand]
    public void Cancel()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty; 
        Password  = string.Empty;
        ConfirmPassword = string.Empty;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    public async Task  RegisterEmployee()
    { 
        if (ConfirmPassword == Password)
        {
            _employee.Firstname = FirstName;
            _employee.Lastname = LastName;     
            _employee.Email = Email;
            _employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
            
            await _employeeRepository.CreateAsync(_employee);
            ErrorMessage = "Employee has been registered successfully";
            Console.WriteLine($"Employee register: {_employee.ToString()} ");
            
            FirstName= string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Password  = string.Empty;
            ConfirmPassword = string.Empty;
            ErrorMessage = string.Empty;
        }
        ConfirmPassword = string.Empty;
        Password = string.Empty;
        ErrorMessage = "Please enter a password";
        
    }
}