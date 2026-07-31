<#
.SYNOPSIS
    Opens PowerShell or CMD at the current active File Explorer location or Desktop.
.DESCRIPTION
    Traverses Z-order windows to find the active File Explorer window or Desktop,
    extracts its exact folder path, and launches PowerShell or CMD at that location.
#>
param(
    [ValidateSet('powershell', 'cmd')]
    [string]$Terminal = 'powershell'
)

Add-Type -TypeDefinition @"
using System;
using System.Text;
using System.Runtime.InteropServices;

public class Win32ActiveExplorerFinder {
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    public const uint GW_HWNDNEXT = 2;

    public static IntPtr FindActiveExplorerHwnd() {
        IntPtr current = GetForegroundWindow();
        int maxDepth = 100;
        IntPtr desktopHwnd = IntPtr.Zero;

        while (current != IntPtr.Zero && maxDepth > 0) {
            if (IsWindowVisible(current)) {
                StringBuilder sb = new StringBuilder(256);
                GetClassName(current, sb, sb.Capacity);
                string cls = sb.ToString();

                if (cls == "CabinetWClass" || cls == "ExploreWClass") {
                    return current;
                }
                if (cls == "Progman" || cls == "WorkerW") {
                    if (desktopHwnd == IntPtr.Zero) desktopHwnd = current;
                }
            }
            current = GetWindow(current, GW_HWNDNEXT);
            maxDepth--;
        }
        return desktopHwnd;
    }

    public static string GetWindowClassName(IntPtr hWnd) {
        if (hWnd == IntPtr.Zero) return "";
        StringBuilder sb = new StringBuilder(256);
        GetClassName(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }
}
"@ -ErrorAction SilentlyContinue

$targetPath = [Environment]::GetFolderPath('UserProfile')

try {
    $activeHwnd = [Win32ActiveExplorerFinder]::FindActiveExplorerHwnd()
    $clsName = [Win32ActiveExplorerFinder]::GetWindowClassName($activeHwnd)

    if ($clsName -eq 'Progman' -or $clsName -eq 'WorkerW') {
        $desktopPath = [Environment]::GetFolderPath('Desktop')
        if (Test-Path -LiteralPath $desktopPath) {
            $targetPath = $desktopPath
        }
    } elseif ($activeHwnd -and $activeHwnd -ne [IntPtr]::Zero) {
        $shell = New-Object -ComObject Shell.Application
        $windows = @($shell.Windows())

        foreach ($w in $windows) {
            try {
                $wHwnd = $w.HWND
                if ($wHwnd -eq $activeHwnd.ToInt64() -or $wHwnd -eq $activeHwnd.ToInt32()) {
                    $p = $w.Document.Folder.Self.Path
                    if ($p -and (Test-Path -LiteralPath $p -PathType Container)) {
                        $targetPath = $p
                        break
                    }
                }
            } catch {}
        }
    }
} catch {}

if ($Terminal -eq 'cmd') {
    Start-Process -FilePath 'cmd.exe' -WorkingDirectory $targetPath
} else {
    Start-Process -FilePath 'powershell.exe' -WorkingDirectory $targetPath
}
