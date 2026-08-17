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
            foreach (var arg in e.Args)
            {
                if (!string.IsNullOrWhiteSpace(arg))
                {
                    try
                    {
                        string fullPath = Path.GetFullPath(arg);
                        mainWindow.OpenFile(fullPath);
                    }
                    catch
                    {
                        // Ignore invalid paths
                    }
                }
            }
        }

        mainWindow.Show();
    }
}
