using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace QuickSymbols;

public partial class App : Application
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private void App_Startup(object sender, StartupEventArgs e)
    {
        IntPtr previousForegroundHwnd = GetForegroundWindow();

        var mainWindow = new MainWindow(previousForegroundHwnd);
        mainWindow.Show();
        mainWindow.Closed += (s, args) => Shutdown();
    }
}
