<#
.SYNOPSIS
    Instantly unmanages the focused dialog or window, shrinks/centers it to native dialog size, and manages persistent GlazeWM ignore rules.
.DESCRIPTION
    Keybindings:
      Alt+Shift+I : Permanently ignore focused dialog, shrink & center it, persist rule in config.yaml.
      Alt+Shift+U : Remove focused dialog from permanent ignore rules in config.yaml & reload.
      Alt+Ctrl+I  : Session-only ignore, shrink & center dialog without writing to config.yaml.

    CLI Options:
      -List       : Display all custom ignored dialog rules.
      -Remove     : Remove the focused window's rule from config.yaml.
      -SessionOnly: Ignore window for current session without modifying config.yaml.
#>

[CmdletBinding()]
param(
    [switch]$SessionOnly,
    [switch]$Remove,
    [switch]$List
)

$configPath = "$env:USERPROFILE\Documents\myenv\glazewm\config.yaml"
if (-not (Test-Path $configPath)) {
    $configPath = "$env:USERPROFILE\.glzr\glazewm\config.yaml"
}

# 1. CLI List Mode
if ($List) {
    if (-not (Test-Path $configPath)) {
        Write-Host "GlazeWM config not found at $configPath" -ForegroundColor Red
        exit 1
    }

    $lines = [System.IO.File]::ReadAllLines($configPath, [System.Text.Encoding]::UTF8)
    $inCustomSection = $false
    $rulesFound = 0

    Write-Host "`n=== GlazeWM Custom Ignored Dialogs ===" -ForegroundColor Cyan
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match "USER CUSTOM IGNORED DIALOGS") {
            $inCustomSection = $true
            continue
        }
        if ($inCustomSection -and ($line -match "^\s*-\s*commands:" -or $line -match "^\s*binding_modes:" -or $line -match "^\s*# Always tile")) {
            $inCustomSection = $false
            break
        }
        if ($inCustomSection -and $line.Trim().StartsWith("# Custom rule:")) {
            $rulesFound++
            Write-Host "[$rulesFound] $($line.Trim().Substring(2))" -ForegroundColor Green
            # Print associated match lines
            for ($j = $i + 1; $j -lt $lines.Count; $j++) {
                if ($lines[$j].Trim().StartsWith("- window_") -or $lines[$j].Trim().StartsWith("window_")) {
                    Write-Host "    $($lines[$j].Trim())" -ForegroundColor Gray
                } else {
                    break
                }
            }
        }
    }

    if ($rulesFound -eq 0) {
        Write-Host "No custom ignored dialogs found." -ForegroundColor Yellow
    }
    Write-Host ""
    exit 0
}

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

# Helper to show dark non-intrusive toast notification
function Show-NotificationToast([string]$titleText, [string]$detailText) {
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
        $header.Text = $titleText
        $header.FontSize = 12
        $header.FontWeight = [System.Windows.FontWeights]::SemiBold
        $header.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(240, 240, 240))
        $stack.Children.Add($header) | Out-Null

        $detail = New-Object System.Windows.Controls.TextBlock
        $detail.Text = $detailText
        $detail.FontSize = 11
        $detail.Margin = New-Object System.Windows.Thickness(0, 4, 0, 0)
        $detail.TextTrimming = [System.Windows.TextTrimming]::CharacterEllipsis
        $detail.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(160, 160, 160))
        $stack.Children.Add($detail) | Out-Null

        $border.Child = $stack
        $win.Content = $border

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
}

try {
    # 2. Query focused window from GlazeWM
    $raw = & glazewm.exe query focused 2>$null
    if (-not $raw) { exit 0 }

    $json = $raw | ConvertFrom-Json
    if (-not $json.success -or -not $json.data.focused) { exit 0 }

    $focused = $json.data.focused
    $procName = if ($focused.processName) { [string]$focused.processName } else { "" }
    $className = if ($focused.className) { [string]$focused.className } else { "" }
    $title = if ($focused.title) { [string]$focused.title } else { "" }
    $windowId = $focused.id
    $hwndVal = $focused.handle

    if (-not $procName -and -not $title -and -not $className) { exit 0 }

    # 3. Handle Removal Mode (-Remove / Alt+Shift+U)
    if ($Remove) {
        if (-not (Test-Path $configPath)) { exit 0 }

        $lines = [System.Collections.Generic.List[string]]([System.IO.File]::ReadAllLines($configPath, [System.Text.Encoding]::UTF8))
        $removed = $false
        $removedTitle = if ($title) { $title.Trim() } else { $procName }

        # Look for matching custom rule under the custom section
        for ($i = $lines.Count - 1; $i -ge 0; $i--) {
            $line = $lines[$i]
            $matchTarget = $false

            if ($title -and ($line -match [regex]::Escape($title.Trim()))) {
                $matchTarget = $true
            } elseif ($procName -and ($line -match "regex: '\(\?i\)\^$([regex]::Escape($procName))\.\*\$'")) {
                $matchTarget = $true
            }

            if ($matchTarget) {
                # Find start of block (comment line above)
                $startIdx = $i
                while ($startIdx -gt 0 -and (-not $lines[$startIdx].Trim().StartsWith("# Custom rule:"))) {
                    $startIdx--
                }
                # Find end of block
                $endIdx = $i
                while ($endIdx + 1 -lt $lines.Count -and ($lines[$endIdx + 1].Trim().StartsWith("window_") -or $lines[$endIdx + 1].Trim().StartsWith("- window_"))) {
                    $endIdx++
                }

                $countToRemove = ($endIdx - $startIdx) + 1
                for ($k = 0; $k -lt $countToRemove; $k++) {
                    $lines.RemoveAt($startIdx)
                }
                $removed = $true
                break
            }
        }

        if ($removed) {
            [System.IO.File]::WriteAllLines($configPath, $lines, [System.Text.Encoding]::UTF8)
            & glazewm.exe command wm-reload-config 2>$null
            try { [System.Media.SystemSounds]::Asterisk.Play() } catch {}
            Show-NotificationToast "GlazeWM: Removed from Ignore List" $removedTitle
        } else {
            Show-NotificationToast "GlazeWM: Rule Not Found" "Window was not in custom ignore list."
        }
        exit 0
    }

    # 4. Immediate unmanage in current GlazeWM session
    if ($windowId) {
        & glazewm.exe command ignore --id $windowId 2>$null
    } else {
        & glazewm.exe command ignore 2>$null
    }

    # 5. Win32 un-maximize and shrink/center dialog
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

    # 6. Formulate Permanent YAML Rule
    if (-not (Test-Path $configPath)) { exit 0 }

    $dateStr = (Get-Date).ToString("yyyy-MM-dd HH:mm")
    $trimmedTitle = $title.Trim()
    $safeTitle = if ($trimmedTitle) { [regex]::Replace($trimmedTitle, '([\\\[\]\(\)\{\}\.\*\+\?\^\$\|])', '\$1') } else { "" }
    $safeProc = if ($procName) { [regex]::Replace($procName, '([\\\[\]\(\)\{\}\.\*\+\?\^\$\|])', '\$1') } else { "" }

    $ruleLines = [System.Collections.Generic.List[string]]::new()
    $displayTitle = if ($trimmedTitle.Length -gt 35) { $trimmedTitle.Substring(0, 32) + "..." } else { $trimmedTitle }
    $ruleSummary = ""

    if ($className -eq "#32770" -and $procName) {
        $ruleSummary = "$procName [Dialog #32770]"
        $ruleLines.Add("      # Custom rule: $procName [Dialog #32770] [Added: $dateStr]")
        $ruleLines.Add("      - window_process: { regex: '(?i)^" + $safeProc + ".*$' }")
        $ruleLines.Add("        window_class: { equals: '#32770' }")
    }
    elseif ($safeTitle -and $procName) {
        $ruleSummary = "$procName ($displayTitle)"
        $ruleLines.Add("      # Custom rule: $displayTitle [Added: $dateStr]")
        $ruleLines.Add("      - window_process: { regex: '(?i)^" + $safeProc + ".*$' }")
        $ruleLines.Add("        window_title: { regex: '(?i).*" + $safeTitle + ".*' }")
    }
    elseif ($className -and $procName) {
        $ruleSummary = "$procName [Class: $className]"
        $ruleLines.Add("      # Custom rule: $procName [Class: $className] [Added: $dateStr]")
        $ruleLines.Add("      - window_process: { regex: '(?i)^" + $safeProc + ".*$' }")
        $ruleLines.Add("        window_class: { equals: '" + $className + "' }")
    }
    elseif ($procName) {
        $ruleSummary = "$procName"
        $ruleLines.Add("      # Custom rule: $procName [Added: $dateStr]")
        $ruleLines.Add("      - window_process: { regex: '(?i)^" + $safeProc + ".*$' }")
    }

    if ($ruleLines.Count -gt 0) {
        $lines = [System.Collections.Generic.List[string]]([System.IO.File]::ReadAllLines($configPath, [System.Text.Encoding]::UTF8))

        # Check if identical match line already exists
        $alreadyPresent = $false
        foreach ($line in $lines) {
            if ($line.Trim() -eq $ruleLines[1].Trim()) {
                $alreadyPresent = $true
                break
            }
        }

        if (-not $alreadyPresent) {
            # Find insertion anchor
            $anchorIdx = $lines.FindIndex([Predicate[string]]{ param($s) $s -match "USER CUSTOM IGNORED DIALOGS" })
            if ($anchorIdx -ge 0) {
                $lines.InsertRange($anchorIdx + 1, $ruleLines)
            } else {
                $anchorIdx = $lines.FindIndex([Predicate[string]]{ param($s) $s -match "myenv-ignore-toast" })
                if ($anchorIdx -ge 0) {
                    $lines.InsertRange($anchorIdx + 1, $ruleLines)
                }
            }

            [System.IO.File]::WriteAllLines($configPath, $lines, [System.Text.Encoding]::UTF8)
            & glazewm.exe command wm-reload-config 2>$null
        } else {
            & glazewm.exe command wm-reload-config 2>$null
        }
    }

    # Audio confirmation
    try { [System.Media.SystemSounds]::Asterisk.Play() } catch {}

    # Notification Toast
    Show-NotificationToast "GlazeWM: Added to Permanent Ignore List" $ruleSummary

} catch {
    # Non-blocking error handling
}
