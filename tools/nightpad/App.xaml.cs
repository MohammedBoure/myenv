using System;
using System.IO;
using System.Windows;

namespace NightPad;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void App_Startup(object sender, StartupEventArgs e)
    {
        var mainWindow = new MainWindow();

        if (e.Args.Length > 0 && !string.IsNullOrWhiteSpace(e.Args[0]))
        {
            try
            {
                string fullPath = Path.GetFullPath(e.Args[0]);
                if (File.Exists(fullPath))
                {
                    mainWindow.OpenFile(fullPath);
                }
            }
            catch
            {
                // Ignore invalid paths
            }
        }

        mainWindow.Show();
    }
}
