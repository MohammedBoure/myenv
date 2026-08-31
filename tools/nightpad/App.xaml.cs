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

        if (e.Args.Length > 0)
        {
            try
            {
                string rawArg = e.Args.Length == 1 ? e.Args[0] : string.Join(" ", e.Args);
                rawArg = rawArg.Trim('"', ' ');
                if (!string.IsNullOrWhiteSpace(rawArg))
                {
                    string fullPath = Path.GetFullPath(rawArg);
                    mainWindow.OpenOrCreateFile(fullPath);
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
