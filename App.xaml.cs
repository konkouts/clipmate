using System.Configuration;
using System.Data;
using System.Windows;

namespace ClipMate;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var tray = new ClipMate.Services.TrayIconService();
        new MainWindow().Show();
    }

}

