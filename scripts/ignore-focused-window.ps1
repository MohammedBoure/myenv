<#
.SYNOPSIS
    Instantly unmanages the focused dialog or window, shrinks/centers it to native dialog size, and persists an ignore rule.
.DESCRIPTION
    Triggered via Alt+Shift+I (permanent rule) or Alt+Ctrl+I (session only).
    1. Queries the active focused window details via GlazeWM CLI.
    2. Immediately calls 'glazewm command ignore' to unmanage and untile the dialog right now.
    3. Restores window from maximized/stretched state, sets clean centered dialog dimensions via Win32.
    4. If not -SessionOnly, appends rule to glazewm/config.yaml under window_rules (ignore) and reloads.
    5. Displays a subtle, non-activating dark toast notification and plays audio confirmation.
#>

[CmdletBinding()]
param(
    [switch]$SessionOnly
)

# Win32 API Helper for un-maximizing, resizing, and centering dialogs
$win32Code = @"
using System;
using System.Runtime.InteropServices;

public class Win32DialogResizer {
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public const int SW_RESTORE = 9;
    public const uint SWP_NOZORDER = 0x0004;
    public const uint SWP_FRAMECHANGED = 0x0020;
    public const uint SWP_SHOWWINDOW = 0x0040;

    public static void ShrinkAndCenter(IntPtr hWnd, int sLeft, int sTop, int sWidth, int sHeight) {
        if (hWnd == IntPtr.Zero) return;
        ShowWindow(hWnd, SW_RESTORE);

        RECT rect;
        GetWindowRect(hWnd, out rect);
        int curW = rect.Right - rect.Left;
        int curH = rect.Bottom - rect.Top;

        int targetW = curW;
        int targetH = curH;

        // If stretched or enlarged near full screen
        if (curW >= sWidth * 0.80 || curH >= sHeight * 0.80 || curW <= 0 || curH <= 0) {
            targetW = Math.Min(860, (int)(sWidth * 0.55));
            targetH = Math.Min(600, (int)(sHeight * 0.65));
        }

        int targetX = sLeft + (sWidth - targetW) / 2;
        int targetY = sTop + (sHeight - targetH) / 2;

        SetWindowPos(hWnd, IntPtr.Zero, targetX, targetY, targetW, targetH, SWP_NOZORDER | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
    }
}
"@

try {
    if (-not ([System.Management.Automation.PSTypeName]'Win32DialogResizer').Type) {
        Add-Type -TypeDefinition $win32Code -ErrorAction SilentlyContinue
    }
} catch {}

try {
    # 1. Query focused window from GlazeWM
    $raw = & glazewm.exe query focused 2>$null
    if (-not $raw) {
        exit 0
    }

    $json = $raw | ConvertFrom-Json
    if (-not $json.success -or -not $json.data.focused) {
        exit 0
    }

    $focused = $json.data.focused
    $procName = if ($focused.processName) { [string]$focused.processName } else { "" }
    $className = if ($focused.className) { [string]$focused.className } else { "" }
    $title = if ($focused.title) { [string]$focused.title } else { "" }
    $windowId = $focused.id
    $hwndVal = $focused.handle

    if (-not $procName -and -not $title -and -not $className) {
        exit 0
    }

    # 2. Immediately execute GlazeWM ignore command to unmanage the dialog instantly
    if ($windowId) {
        & glazewm.exe command ignore --id $windowId 2>$null
    } else {
        & glazewm.exe command ignore 2>$null
    }

    # 3. Restore window from maximized/stretched state and center with clean dialog dimensions
    if ($hwndVal) {
        try {
            Add-Type -AssemblyName PresentationFramework -ErrorAction SilentlyContinue
            $screen = [System.Windows.SystemParameters]::WorkArea
            $hWnd = [IntPtr][long]$hwndVal
            [Win32DialogResizer]::ShrinkAndCenter($hWnd, [int]$screen.Left, [int]$screen.Top, [int]$screen.Width, [int]$screen.Height)
        } catch {}
    }

    # If user only wanted session-only ignore, play sound and exit
    if ($SessionOnly) {
        try { [System.Media.SystemSounds]::Asterisk.Play() } catch {}
        exit 0
    }

    # 4. Locate GlazeWM config file
    $configPath = "$env:USERPROFILE\Documents\myenv\glazewm\config.yaml"
    if (-not (Test-Path $configPath)) {
        $configPath = "$env:USERPROFILE\.glzr\glazewm\config.yaml"
    }
    if (-not (Test-Path $configPath)) {
        exit 0
    }

    $configContent = [System.IO.File]::ReadAllText($configPath, [System.Text.Encoding]::UTF8)

    # 5. Formulate precise matching rule
    $dateStr = (Get-Date).ToString("yyyy-MM-dd HH:mm")
    $ruleSummary = ""
    $ruleLines = @()

    # Clean title for regex matching
    $trimmedTitle = $title.Trim()
    $safeTitleRegex = ""
    if ($trimmedTitle) {
        if ($trimmedTitle.Length -gt 40) {
            $trimmedTitle = $trimmedTitle.Substring(0, 40)
        }
        $safeTitleRegex = [regex]::Escape($trimmedTitle)
    }

    if ($className -eq "#32770" -and $procName) {
        $ruleSummary = "$procName [Dialog #32770]"
        $ruleLines = @(
            "      # Ignored standard dialog (#32770) added on $($dateStr)",
            "      - window_process: { regex: '(?i)^$([regex]::Escape($procName))$' }",
            "        window_class: { equals: '#32770' }"
        )
    }
    elseif ($safeTitleRegex -and $procName) {
        $displayTitle = if ($title.Length -gt 35) { $title.Substring(0, 32) + "..." } else { $title }
        $ruleSummary = "$procName ($displayTitle)"
        $ruleLines = @(
            "      # Ignored dialog/window added on $($dateStr) - $displayTitle",
            "      - window_process: { regex: '(?i)^$([regex]::Escape($procName))$' }",
            "        window_title: { regex: '(?i).*$safeTitleRegex.*' }"
        )
    }
    elseif ($className -and $procName) {
        $ruleSummary = "$procName [Class: $className]"
        $ruleLines = @(
            "      # Ignored dialog window added on $($dateStr) - Class $className",
            "      - window_process: { regex: '(?i)^$([regex]::Escape($procName))$' }",
            "        window_class: { equals: '$className' }"
        )
    }
    elseif ($procName) {
        $ruleSummary = "$procName"
        $ruleLines = @(
            "      # Ignored application added on $($dateStr) - $procName",
            "      - window_process: { regex: '(?i)^$([regex]::Escape($procName))$' }"
        )
    }

    if ($ruleLines.Count -gt 0) {
        $newRuleText = ($ruleLines -join "`r`n")

        # Check if already present in config
        if (-not ($configContent -match [regex]::Escape($ruleLines[1].Trim()))) {
            # Find insertion anchor in window_rules -> commands: ['ignore']
            $anchorRegex = "(?m)^(\s*-\s*window_title:\s*\{\s*regex:\s*['`"].*myenv-app-launcher.*['`"]\s*\})"
            if ($configContent -match $anchorRegex) {
                $replacement = "`$1`r`n$newRuleText"
                $newConfig = [regex]::Replace($configContent, $anchorRegex, $replacement, 1)
                [System.IO.File]::WriteAllText($configPath, $newConfig, [System.Text.Encoding]::UTF8)

                # Reload GlazeWM config
                & glazewm.exe command wm-reload-config 2>$null
            }
        }
    }

    # Audio confirmation
    try { [System.Media.SystemSounds]::Asterisk.Play() } catch {}

    # 6. Fast non-intrusive notification toast (WPF)
    try {
        Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase -ErrorAction SilentlyContinue

        $win = New-Object System.Windows.Window
        $win.Title = "myenv-ignore-toast"
        $win.WindowStyle = [System.Windows.WindowStyle]::None
        $win.AllowsTransparency = $true
        $win.Background = [System.Windows.Media.Brushes]::Transparent
        $win.Topmost = $true
        $win.ShowActivated = $false
        $win.ShowInTaskbar = $false
        $win.Focusable = $false
        $win.Width = 360
        $win.Height = 72

        # Position at bottom-right corner above taskbar
        $screen = [System.Windows.SystemParameters]::WorkArea
        $win.Left = $screen.Right - 380
        $win.Top = $screen.Bottom - 85

        $border = New-Object System.Windows.Controls.Border
        $border.CornerRadius = New-Object System.Windows.CornerRadius(0)
        $border.BorderThickness = New-Object System.Windows.Thickness(1)
        $border.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(60, 60, 60))
        $border.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(248, 15, 15, 15))
        $border.Padding = New-Object System.Windows.Thickness(14, 10, 14, 10)

        $stack = New-Object System.Windows.Controls.StackPanel

        $header = New-Object System.Windows.Controls.TextBlock
        $header.Text = "GlazeWM: Window Added to Ignore List"
        $header.FontSize = 12
        $header.FontWeight = [System.Windows.FontWeights]::SemiBold
        $header.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(240, 240, 240))
        $stack.Children.Add($header) | Out-Null

        $detail = New-Object System.Windows.Controls.TextBlock
        $detail.Text = $ruleSummary
        $detail.FontSize = 11
        $detail.Margin = New-Object System.Windows.Thickness(0, 4, 0, 0)
        $detail.TextTrimming = [System.Windows.TextTrimming]::CharacterEllipsis
        $detail.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(160, 160, 160))
        $stack.Children.Add($detail) | Out-Null

        $border.Child = $stack
        $win.Content = $border

        # Auto-close after 2.2 seconds
        $timer = New-Object System.Windows.Threading.DispatcherTimer
        $timer.Interval = [TimeSpan]::FromSeconds(2.2)
        $timer.Add_Tick({
            $win.Close()
            [System.Windows.Threading.Dispatcher]::CurrentDispatcher.InvokeShutdown()
        })
        $timer.Start()

        $win.Show()
        [System.Windows.Threading.Dispatcher]::Run()
    } catch {}

} catch {
    # Non-blocking error handling
}
