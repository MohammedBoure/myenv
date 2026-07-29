Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
using System.Text;

public class WinTest {
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
"@

$hwnd = [WinTest]::GetForegroundWindow()
$cls = New-Object System.Text.StringBuilder 256
$title = New-Object System.Text.StringBuilder 256
[WinTest]::GetClassName($hwnd, $cls, 256) | Out-Null
[WinTest]::GetWindowText($hwnd, $title, 256) | Out-Null
$rect = New-Object WinTest+RECT
[WinTest]::GetWindowRect($hwnd, [ref]$rect) | Out-Null

Write-Host "HWND: $hwnd"
Write-Host "Class: $($cls.ToString())"
Write-Host "Title: $($title.ToString())"
Write-Host "Rect: Left=$($rect.Left), Top=$($rect.Top), Right=$($rect.Right), Bottom=$($rect.Bottom)"
