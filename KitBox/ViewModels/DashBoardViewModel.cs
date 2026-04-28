using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KitBox.DataAccess;
using KitBox.DataAccess.Interfaces;
using KitBox.DataAccess.Repositories;
using KitBox.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;


namespace KitBox.ViewModels;

/// <summary>
/// Le Backend pour gerer les connexions.
/// </summary>
public partial class DashBoardViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private readonly DatabaseConnection _db;
    /// private readonly IEmployeeRepository _employeeRepository;
    [ObservableProperty] private int totalUsers = 754;
    [ObservableProperty] private int newUsers = 189;
    [ObservableProperty] private double bounceRate = 76.5;
    [ObservableProperty] private string totalUsersTrend = "+7.5%";
    [ObservableProperty] private double newUsersTrend = 45.2;
    [ObservableProperty] private string timeRange = "Day";


    public DashBoardViewModel(MainViewModel main)
    {
        _main = main;
        var db = new DatabaseConnection(
            server: EnvConfig.Get("DB_SERVER", "localhost"),
            database: EnvConfig.Get("DB_NAME", "kitbox"),
            user : EnvConfig.Get("DB_USER",  "root"),
            port  : int.Parse(EnvConfig.Get("DB_PORT",  "5432")),
            password : EnvConfig.Get("DB_PASSWORD",  ""));
        _db = db;
    }
    
}    