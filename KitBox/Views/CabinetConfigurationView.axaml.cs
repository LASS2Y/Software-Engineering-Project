using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaWebView;
using KitBox.ViewModels;
using System;
using System.IO;
using System.Reflection;

namespace KitBox.Views;

public partial class CabinetConfigurationView : UserControl
{
    private WebView? _webView;

    public CabinetConfigurationView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _webView = this.FindControl<WebView>("Cabinet3DView");

        // ✅ Chemin absolu vers le fichier HTML dans le dossier de l'exe
        var exeDir  = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var htmlPath = Path.Combine(exeDir, "Assets", "Web", "index.html");

        System.Console.WriteLine($"HTML path: {htmlPath}");
        System.Console.WriteLine($"File exists: {File.Exists(htmlPath)}");

        if (_webView != null && File.Exists(htmlPath))
        {
            _webView.Url = new Uri(htmlPath);
        }

        if (DataContext is CabinetConfigurationViewModel vm)
        {
            vm.OnRefresh3D += UpdateWebView;

            System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => vm.TriggerRefresh());
            });
        }
    }

    private void UpdateWebView(string json)
    {
        _webView?.ExecuteScriptAsync($"updateCabinet('{json.Replace("'", "\\'")}')");
    }
}