<#
.SYNOPSIS
    Opens PowerShell or CMD at the current File Explorer location.
.DESCRIPTION
    Detects the active File Explorer window, extracts its current folder path,
    and launches a new terminal (PowerShell or CMD) at that location.
    If no File Explorer window is focused, defaults to the user's home directory.
#>
param(
    [ValidateSet('powershell', 'cmd')]
    [string]$Terminal = 'powershell'
)

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class ExplorerHelper {
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();
}
"@ -ErrorAction SilentlyContinue

$targetPath = [Environment]::GetFolderPath('UserProfile')

try {
    $fgHwnd = [ExplorerHelper]::GetForegroundWindow()
    if ($fgHwnd -and $fgHwnd -ne [IntPtr]::Zero) {
        $shell = New-Object -ComObject Shell.Application
        foreach ($window in $shell.Windows()) {
            if ($window.HWND -eq $fgHwnd.ToInt64() -or $window.HWND -eq $fgHwnd.ToInt32()) {
                $folderPath = $window.Document.Folder.Self.Path
                if ($folderPath -and (Test-Path -LiteralPath $folderPath -PathType Container)) {
                    $targetPath = $folderPath
                }
                break
            }
        }
    }
} catch {}

if ($Terminal -eq 'cmd') {
    Start-Process -FilePath 'cmd.exe' -WorkingDirectory $targetPath
} else {
    Start-Process -FilePath 'powershell.exe' -WorkingDirectory $targetPath
}
