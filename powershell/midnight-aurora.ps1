# myenv - Midnight Aurora native PowerShell theme
# No external prompt dependency required.

$script:MyEnvTheme = @{
    Cyan = [ConsoleColor]::Cyan
    Blue = [ConsoleColor]::Blue
    DarkBlue = [ConsoleColor]::DarkBlue
    Green = [ConsoleColor]::Green
    Yellow = [ConsoleColor]::Yellow
    Magenta = [ConsoleColor]::Magenta
    Gray = [ConsoleColor]::DarkGray
    White = [ConsoleColor]::White
}

try {
    # Ensure myenv tools (fzf) are in PATH
    $fzfToolsPath = "$env:USERPROFILE\Documents\myenv\tools\fzf"
    if (Test-Path $fzfToolsPath) {
        if ($env:PATH -notlike "*$fzfToolsPath*") {
            $env:PATH = "$fzfToolsPath;$env:PATH"
        }
    }

    Set-PSReadLineOption -EditMode Windows
    Set-PSReadLineOption -PredictionSource History
    Set-PSReadLineOption -PredictionViewStyle ListView
    Set-PSReadLineOption -BellStyle None
    Set-PSReadLineKeyHandler -Key Tab -Function MenuComplete
    Set-PSReadLineKeyHandler -Key Ctrl+Backspace -Function BackwardKillWord
    Set-PSReadLineKeyHandler -Key Ctrl+v -Function Paste
    Set-PSReadLineKeyHandler -Key Ctrl+c -Function CopyOrCancelLine

    # FZF Interactive History Search (Ctrl+R)
    Set-PSReadLineKeyHandler -Chord 'Ctrl+r' -ScriptBlock {
        $histPath = (Get-PSReadLineOption).HistorySavePath
        if (Test-Path $histPath) {
            $selected = Get-Content $histPath -Encoding UTF8 | Select-Object -Unique | fzf --height 40% --layout=reverse --prompt="Search History > "
            if ($selected) {
                [Microsoft.PowerShell.PSConsoleReadLine]::RevertLine()
                [Microsoft.PowerShell.PSConsoleReadLine]::Insert($selected)
            }
        }
    }

    # FZF Interactive File Search (Ctrl+T)
    Set-PSReadLineKeyHandler -Chord 'Ctrl+t' -ScriptBlock {
        $selected = Get-ChildItem -Recurse -File -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName | fzf --height 40% --layout=reverse --prompt="Search Files > "
        if ($selected) {
            [Microsoft.PowerShell.PSConsoleReadLine]::Insert("`"$selected`"")
        }
    }
    Set-PSReadLineOption -Colors @{
        Command   = 'Cyan'
        Parameter = 'Yellow'
        String    = 'Green'
        Operator  = 'DarkCyan'
        Variable  = 'Magenta'
        Comment   = 'DarkGray'
        Keyword   = 'Blue'
        Type      = 'DarkYellow'
        Number    = 'DarkGreen'
        Member    = 'White'
        InlinePrediction = 'DarkGray'
    }
} catch {
    # Keep the profile compatible with older PSReadLine hosts.
}

function Get-MyEnvGitBranch {
    try {
        $branch = git branch --show-current 2>$null
        if ($LASTEXITCODE -eq 0 -and $branch) {
            return "  <$branch>"
        }
    } catch {}
    return ''
}

function prompt {
    $location = (Get-Location).Path
    if ($location.StartsWith($HOME, [System.StringComparison]::OrdinalIgnoreCase)) {
        $location = '~' + $location.Substring($HOME.Length)
    }

    $time = Get-Date -Format 'HH:mm'
    $branch = Get-MyEnvGitBranch

    Write-Host ''
    Write-Host ('  ' + $time + '  ') -ForegroundColor DarkGray -NoNewline
    Write-Host 'moham' -ForegroundColor Cyan -NoNewline
    Write-Host '@' -ForegroundColor DarkGray -NoNewline
    Write-Host $env:COMPUTERNAME -ForegroundColor Blue -NoNewline
    Write-Host '  ' -NoNewline
    Write-Host $location -ForegroundColor White -NoNewline
    if ($branch) {
        Write-Host $branch -ForegroundColor Magenta -NoNewline
    }
    Write-Host ''
    Write-Host '  >> ' -ForegroundColor Cyan -NoNewline
    return ''
}

Remove-Item -Path Alias:cd -Force -ErrorAction SilentlyContinue
Remove-Item -Path Alias:chdir -Force -ErrorAction SilentlyContinue

function cd {
    if ($args.Count -eq 0) {
        Set-Location $HOME
    } else {
        Set-Location @args
    }
    if ($?) {
        Get-ChildItem
    }
}
function chdir {
    if ($args.Count -eq 0) {
        Set-Location $HOME
    } else {
        Set-Location @args
    }
    if ($?) {
        Get-ChildItem
    }
}

function ll { Get-ChildItem -Force | Format-Table Mode,LastWriteTime,Length,Name -AutoSize }
function la { Get-ChildItem -Force }
function gs { git status }
function croot { cd $HOME }

# Interactive Documentation & Cheat Sheet Navigator
function docs {
    param([string]$Topic = "")
    $docsScript = "$env:USERPROFILE\Documents\myenv\scripts\docs.ps1"
    if (Test-Path $docsScript) {
        & $docsScript $Topic
    } else {
        Write-Host "Docs script not found at $docsScript" -ForegroundColor Red
    }
}

# Copy Command Output to Clipboard while displaying in Terminal
function cb {
    begin {
        $pipelineItems = [System.Collections.Generic.List[string]]::new()
    }
    process {
        if ($_) {
            $str = Out-String -InputObject $_
            Write-Output $_
            $pipelineItems.Add($str.TrimEnd())
        }
    }
    end {
        if ($args.Count -gt 0) {
            $cmdStr = $args -join ' '
            $out = Invoke-Expression $cmdStr | Tee-Object -Variable _cbCaptured
            if ($_cbCaptured) {
                ($_cbCaptured | Out-String).TrimEnd() | Set-Clipboard
            }
        } elseif ($pipelineItems.Count -gt 0) {
            ($pipelineItems -join "`r`n").TrimEnd() | Set-Clipboard
        }
    }
}
Set-Alias -Name c -Value cb -Option ReadOnly, AllScope -ErrorAction SilentlyContinue