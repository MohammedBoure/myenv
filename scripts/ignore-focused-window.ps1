<#
.SYNOPSIS
    Toggles the ignore state of the focused window/dialog in GlazeWM and persists rules in config.yaml.
.DESCRIPTION
    Keybinding:
      Alt+Shift+I : Toggle permanent ignore rule for focused window.
                    - If rule exists: Removes rule from config.yaml, reloads WM, shows (Tiled).
                    - If rule does not exist: Adds rule to config.yaml, shrinks & centers, reloads WM, shows (Dialog).

    CLI Options:
      -List       : Display all custom ignored dialog rules.
      -SessionOnly: Unmanage and center dialog for current session without modifying config.yaml.
#>

[CmdletBinding()]
param(
    [switch]$SessionOnly,
    [switch]$List
)

$configPath = "$env:USERPROFILE\Documents\myenv\glazewm\config.yaml"
if (-not (Test-Path $configPath)) {
    $configPath = "$env:USERPROFILE\.glzr\glazewm\config.yaml"
}

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

# 1. CLI List Mode
if ($List) {
    if (-not (Test-Path $configPath)) {
        Write-Host "GlazeWM config not found at $configPath" -ForegroundColor Red
        exit 1
    }

    $lines = [System.IO.File]::ReadAllLines($configPath, $utf8NoBom)
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

# Win32 API Definitions for window detection, placement, and centering
$win32Code = @"
using System;
using System.Text;
using System.Runtime.InteropServices;

public class Win32WindowDetect {
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetWindowTextW(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetClassNameW(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

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
    if (-not ([System.Management.Automation.PSTypeName]'Win32WindowDetect').Type) {
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
        $win.Width = 370
        $win.Height = 72

        $screen = [System.Windows.SystemParameters]::WorkArea
        $win.Left = $screen.Right - 390
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

# Helper to test if a block matches window attributes
function Test-RuleBlockMatch([System.Collections.Generic.List[string]]$Lines, [int]$Start, [int]$End, [string]$Proc, [string]$Title, [string]$Class) {
    $procMatched = $false
    $titleMatched = $false
    $hasTitleRule = $false
    $classMatched = $false
    $hasClassRule = $false

    for ($idx = $Start; $idx -le $End; $idx++) {
        $l = $Lines[$idx].Trim()

        if ($l -match "window_process:\s*\{\s*regex:\s*['`"](?:\(\?i\)\^)?(.*?)(?:\.\*\$|\$)?['`"]\s*\}") {
            $pPattern = $Matches[1]
            if ($Proc -match "(?i)^$([regex]::Escape($pPattern))") {
                $procMatched = $true
            }
        } elseif ($l -match "window_process:\s*\{\s*equals:\s*['`"](.*?)['`"]\s*\}") {
            $pVal = $Matches[1]
            if ($Proc.Equals($pVal, [System.StringComparison]::OrdinalIgnoreCase)) {
                $procMatched = $true
            }
        }

        if ($l -match "window_title:\s*\{\s*regex:\s*['`"](?:\(\?i\)\.\*)?(.*?)(?:\.\*)?['`"]\s*\}") {
            $hasTitleRule = $true
            $tPattern = $Matches[1]
            try {
                if ($Title -match "(?i)$tPattern" -or $Title -match "(?i)$([regex]::Escape($tPattern))") {
                    $titleMatched = $true
                }
            } catch {
                if ($Title.IndexOf($tPattern, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
                    $titleMatched = $true
                }
            }
        } elseif ($l -match "window_title:\s*\{\s*equals:\s*['`"](.*?)['`"]\s*\}") {
            $hasTitleRule = $true
            $tVal = $Matches[1]
            if ($Title.Equals($tVal, [System.StringComparison]::OrdinalIgnoreCase)) {
                $titleMatched = $true
            }
        }

        if ($l -match "window_class:\s*\{\s*equals:\s*['`"](.*?)['`"]\s*\}") {
            $hasClassRule = $true
            $cVal = $Matches[1]
            if ($Class.Equals($cVal, [System.StringComparison]::OrdinalIgnoreCase)) {
                $classMatched = $true
            }
        }
    }

    if (-not $titleMatched -and $Title) {
        $firstLine = $Lines[$Start].Trim()
        if ($firstLine.StartsWith("# Custom rule:")) {
            $ruleName = $firstLine.Replace("# Custom rule:", "").Trim()
            if ($ruleName.IndexOf("[Added:") -gt 0) {
                $ruleName = $ruleName.Substring(0, $ruleName.IndexOf("[Added:")).Trim()
            }
            if ($ruleName -and ($Title.IndexOf($ruleName, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or $ruleName.IndexOf($Title, [System.StringComparison]::OrdinalIgnoreCase) -ge 0)) {
                $titleMatched = $true
            }
        }
    }

    if ($procMatched) {
        if ($hasTitleRule) {
            return $titleMatched
        }
        if ($hasClassRule) {
            return $classMatched
        }
        return $true
    }
    return $false
}

try {
    # 2. Reliable Window Detection
    $procName = ""
    $className = ""
    $title = ""
    $windowId = $null
    $hwndVal = 0

    # Strategy A: Check GlazeWM query focused
    try {
        $raw = & glazewm.exe query focused 2>$null
        if ($raw) {
            $json = $raw | ConvertFrom-Json
            if ($json.success -and $json.data.focused -and $json.data.focused.type -eq "window") {
                $focused = $json.data.focused
                $procName = if ($focused.processName) { [string]$focused.processName } else { "" }
                $className = if ($focused.className) { [string]$focused.className } else { "" }
                $title = if ($focused.title) { [string]$focused.title } else { "" }
                $windowId = $focused.id
                $hwndVal = $focused.handle
            }
        }
    } catch {}

    # Strategy B: Fallback to native Win32 foreground window (essential for already-ignored windows)
    if (-not $procName -or -not $title) {
        try {
            $fgHwnd = [Win32WindowDetect]::GetForegroundWindow()
            if ($fgHwnd -ne [IntPtr]::Zero) {
                $hwndVal = $fgHwnd.ToInt64()

                $sbTitle = New-Object System.Text.StringBuilder 512
                [Win32WindowDetect]::GetWindowTextW($fgHwnd, $sbTitle, 512) | Out-Null
                $nativeTitle = $sbTitle.ToString()
                if (-not $title -and $nativeTitle) { $title = $nativeTitle }

                $sbClass = New-Object System.Text.StringBuilder 256
                [Win32WindowDetect]::GetClassNameW($fgHwnd, $sbClass, 256) | Out-Null
                $nativeClass = $sbClass.ToString()
                if (-not $className -and $nativeClass) { $className = $nativeClass }

                $pidVal = 0
                [Win32WindowDetect]::GetWindowThreadProcessId($fgHwnd, [ref]$pidVal) | Out-Null
                if ($pidVal -gt 0) {
                    try {
                        $nativeProc = [System.Diagnostics.Process]::GetProcessById($pidVal).ProcessName
                        if (-not $procName -and $nativeProc) { $procName = $nativeProc }
                    } catch {}
                }
            }
        } catch {}
    }

    if (-not $procName -and -not $title -and -not $className) { exit 0 }

    # Exclude system shells
    if ($className -in @("Progman", "WorkerW", "Shell_TrayWnd", "Shell_SecondaryTrayWnd") -or ($procName -eq "explorer" -and (-not $title -or $title -eq "Program Manager"))) {
        exit 0
    }

    # 3. Session-Only Mode
    if ($SessionOnly) {
        if ($windowId) {
            & glazewm.exe command ignore --id $windowId 2>$null
        } else {
            & glazewm.exe command ignore 2>$null
        }

        if ($hwndVal) {
            try {
                Add-Type -AssemblyName PresentationFramework -ErrorAction SilentlyContinue
                $screen = [System.Windows.SystemParameters]::WorkArea
                $hWnd = [IntPtr][long]$hwndVal
                [Win32WindowDetect]::ShrinkAndCenter($hWnd, [int]$screen.Left, [int]$screen.Top, [int]$screen.Width, [int]$screen.Height)
            } catch {}
        }

        try { [System.Media.SystemSounds]::Asterisk.Play() } catch {}
        exit 0
    }

    # 4. Check for Existing Rule under USER CUSTOM IGNORED DIALOGS
    if (-not (Test-Path $configPath)) { exit 0 }

    $lines = [System.Collections.Generic.List[string]]([System.IO.File]::ReadAllLines($configPath, $utf8NoBom))
    $customStart = -1
    $customEnd = $lines.Count

    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match "USER CUSTOM IGNORED DIALOGS") {
            $customStart = $i + 1
            break
        }
    }

    if ($customStart -ge 0) {
        for ($i = $customStart; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match "^\s*-\s*commands:" -or $lines[$i] -match "^\s*binding_modes:" -or $lines[$i] -match "^\s*# Always tile") {
                $customEnd = $i
                break
            }
        }
    }

    $existingRuleStart = -1
    $existingRuleEnd = -1

    if ($customStart -ge 0 -and $customStart -lt $customEnd) {
        $blockStart = -1
        for ($i = $customStart; $i -lt $customEnd; $i++) {
            $line = $lines[$i].Trim()
            if ($line.StartsWith("# Custom rule:") -or ($line.StartsWith("- window_") -and $blockStart -eq -1)) {
                if ($blockStart -ne -1) {
                    if (Test-RuleBlockMatch -Lines $lines -Start $blockStart -End ($i - 1) -Proc $procName -Title $title -Class $className) {
                        $existingRuleStart = $blockStart
                        $existingRuleEnd = $i - 1
                        break
                    }
                }
                $blockStart = $i
            }
        }
        if ($existingRuleStart -eq -1 -and $blockStart -ne -1) {
            if (Test-RuleBlockMatch -Lines $lines -Start $blockStart -End ($customEnd - 1) -Proc $procName -Title $title -Class $className) {
                $existingRuleStart = $blockStart
                $existingRuleEnd = $customEnd - 1
            }
        }
    }

    $trimmedTitle = $title.Trim()
    $displayTitle = if ($trimmedTitle.Length -gt 35) { $trimmedTitle.Substring(0, 32) + "..." } else { $trimmedTitle }
    if (-not $displayTitle) { $displayTitle = $procName }

    # =========================================================================
    # CASE A: Rule Exists -> TOGGLE OFF (Remove rule, reload, tile window)
    # =========================================================================
    if ($existingRuleStart -ne -1) {
        $countToRemove = ($existingRuleEnd - $existingRuleStart) + 1
        for ($k = 0; $k -lt $countToRemove; $k++) {
            $lines.RemoveAt($existingRuleStart)
        }

        [System.IO.File]::WriteAllLines($configPath, $lines, $utf8NoBom)

        # Reload GlazeWM and re-tile window
        & glazewm.exe command wm-reload-config 2>$null
        & glazewm.exe command wm-redraw 2>$null
        & glazewm.exe command set-tiling 2>$null

        try { [System.Media.SystemSounds]::Asterisk.Play() } catch {}
        Show-NotificationToast "GlazeWM: Removed from Ignore List (Tiled)" "$procName ($displayTitle)"
        exit 0
    }

    # =========================================================================
    # CASE B: Rule Does Not Exist -> TOGGLE ON (Add rule, unmanage & center)
    # =========================================================================

    # 1. Unmanage window in current GlazeWM session
    if ($windowId) {
        & glazewm.exe command ignore --id $windowId 2>$null
    } else {
        & glazewm.exe command ignore 2>$null
    }

    # 2. Win32 un-maximize and shrink/center dialog
    if ($hwndVal) {
        try {
            Add-Type -AssemblyName PresentationFramework -ErrorAction SilentlyContinue
            $screen = [System.Windows.SystemParameters]::WorkArea
            $hWnd = [IntPtr][long]$hwndVal
            [Win32WindowDetect]::ShrinkAndCenter($hWnd, [int]$screen.Left, [int]$screen.Top, [int]$screen.Width, [int]$screen.Height)
        } catch {}
    }

    # 3. Formulate rule lines (Unicode safe, escaped regex metacharacters)
    $dateStr = (Get-Date).ToString("yyyy-MM-dd HH:mm")
    $safeTitle = if ($trimmedTitle) { [regex]::Replace($trimmedTitle, '([\\\[\]\(\)\{\}\.\*\+\?\^\$\|])', '\$1') } else { "" }
    $safeProc = if ($procName) { [regex]::Replace($procName, '([\\\[\]\(\)\{\}\.\*\+\?\^\$\|])', '\$1') } else { "" }

    $ruleLines = [System.Collections.Generic.List[string]]::new()
    $ruleLines.Add("      # Custom rule: $displayTitle [Added: $dateStr]")

    if ($className -eq "#32770" -and $procName) {
        $ruleLines.Add("      - window_process: { regex: '(?i)^" + $safeProc + ".*$' }")
        $ruleLines.Add("        window_class: { equals: '#32770' }")
    }
    elseif ($safeTitle -and $procName) {
        $ruleLines.Add("      - window_process: { regex: '(?i)^" + $safeProc + ".*$' }")
        $ruleLines.Add("        window_title: { regex: '(?i).*" + $safeTitle + ".*' }")
    }
    elseif ($className -and $procName) {
        $ruleLines.Add("      - window_process: { regex: '(?i)^" + $safeProc + ".*$' }")
        $ruleLines.Add("        window_class: { equals: '" + $className + "' }")
    }
    elseif ($procName) {
        $ruleLines.Add("      - window_process: { regex: '(?i)^" + $safeProc + ".*$' }")
    }

    # 4. Insert rule into config.yaml
    $anchorIdx = $lines.FindIndex([Predicate[string]]{ param($s) $s -match "USER CUSTOM IGNORED DIALOGS" })
    if ($anchorIdx -ge 0) {
        $lines.InsertRange($anchorIdx + 1, $ruleLines)
    } else {
        $anchorIdx = $lines.FindIndex([Predicate[string]]{ param($s) $s -match "myenv-ignore-toast" })
        if ($anchorIdx -ge 0) {
            $lines.InsertRange($anchorIdx + 1, $ruleLines)
        }
    }

    [System.IO.File]::WriteAllLines($configPath, $lines, $utf8NoBom)

    # 5. Reload GlazeWM
    & glazewm.exe command wm-reload-config 2>$null

    try { [System.Media.SystemSounds]::Asterisk.Play() } catch {}
    Show-NotificationToast "GlazeWM: Added to Ignore List (Dialog)" "$procName ($displayTitle)"

} catch {
    # Non-blocking error handling
}
