using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KitBox.DataAccess;
using KitBox.DataAccess.Interfaces;
using KitBox.DataAccess.Repositories;
using KitBox.Models;

namespace KitBox.ViewModels;

/// <summary>
/// Le Backend pour gerer les connexions.
/// </summary>
public partial class CustomerSelectionViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private readonly DatabaseConnection _db;
    private readonly IEmployeeRepository _employeeRepository;

    public CustomerSelectionViewModel(MainViewModel main)
    {
        _main = main;
        var db = new DatabaseConnection(
            server:   EnvConfig.Get("DB_SERVER",   "localhost"),
            database: EnvConfig.Get("DB_NAME",     "kitbox"),
            user:     EnvConfig.Get("DB_USER",     "root"),
            password: EnvConfig.Get("DB_PASSWORD", ""),
            port:     int.Parse(EnvConfig.Get("DB_PORT", "3306")));
        _db = db;
        _employeeRepository = new EmployeeRepository(_db);
    }

   
    [ObservableProperty] private string _email     = string.Empty;
    [ObservableProperty] private string _password   = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _rememberMe ;
    
    [RelayCommand]
    private async Task Login()
    {
        ErrorMessage = "";
        RememberMe = false;
        
       try
       {
           
           if (string.IsNullOrWhiteSpace(Email))
           {
               ErrorMessage = "L'email est requis.";
               return;
           }

           if (string.IsNullOrWhiteSpace(Password))
           {
               ErrorMessage = "Le mot de passe est requis.";
               return;
           }

           // Récupérer l'utilisateur par email
           var employee = await _employeeRepository.GetByEmailAsync(Email);

           if (employee == null)
           {
               ErrorMessage = "Email ou mot de passe incorrect.";
               return;
           }
            
           // Vérifier le mot de passe haché
           if (string.IsNullOrEmpty(employee.PasswordHash) ||
               !_employeeRepository.VerifyPassword(Password, employee.PasswordHash))
           {
               
               ErrorMessage = "Email ou mot de passe incorrect.";
               return;
           }
           // ✅ Connexion réussie
           Console.WriteLine($"✅ Connexion réussie : {employee.Firstname} {employee.Lastname}");
           ErrorMessage = "Connexion réussie ! Redirection...";
            
           // Garde le Password : 
           string A = Password;
           // Ici, tu peux naviguer vers la page principale
           _main.NavigateTo(new NewSecretaryMenuViewModel(_main));
           // await Navigation.NavigateToAsync<HomePageViewModel>();

           if (RememberMe = true)
           {
               employee.Email = Email;
               Password = A;
           }
       }
       catch (Exception ex)
       {
           ErrorMessage = $"Erreur : {ex.Message}";
           Console.WriteLine($"❌ Erreur de connexion : {ex}");
       }
       
    }

    /// <summary>
    /// 
    /// </summary>
   

    [RelayCommand]
    public void GoToWelcomePage()
        => _main.GoToWelcomePage();
}
