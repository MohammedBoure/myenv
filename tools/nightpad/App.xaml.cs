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
                        if (Directory.Exists(fullPath))
                        {
                            mainWindow.OpenFolder(fullPath);
                        }
                        else
                        {
                            mainWindow.OpenFile(fullPath);
                        }
                    }
                    catch
                    {
                        // Ignore invalid paths
                    }
                }
            }
        }
        else
        {
            string currentDir = Environment.CurrentDirectory;
            if (!string.IsNullOrEmpty(currentDir) && Directory.Exists(currentDir))
            {
                mainWindow.OpenFolder(currentDir);
            }
        }

        mainWindow.Show();
    }
}
