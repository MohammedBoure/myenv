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

# Copy Path via fzf - Interactively select a file or folder and copy its normalized relative path
function cpf {
    <#
    .SYNOPSIS
        Interactively search and select a file/folder with fzf, then copy its normalized relative path to the clipboard.
    .DESCRIPTION
        Launches fzf to interactively search and select a file or folder starting from the current working directory.
        The selected path is converted to a relative path, normalized with forward slashes ('/'), stripped of
        leading './' or '.\', and copied to the Windows clipboard via Set-Clipboard.
    .PARAMETER Path
        Optional root folder to search from. Supports Tab completion. Defaults to the current directory ('.').
    .PARAMETER Directory
        Search and select folders/directories only.
    .PARAMETER Help
        Display the shortcuts and usage guide.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory = $false)]
        [string]$Path = '.',

        [Alias('d', 'FoldersOnly')]
        [switch]$Directory,

        [Alias('h', '?')]
        [switch]$Help
    )

    # Display Help & Shortcuts Guide if requested
    if ($Help -or ($Path -in @('-h', '--help', '-?', 'help', '/?', '/h'))) {
        Write-Host ''
        Write-Host '==========================================================' -ForegroundColor DarkGray
        Write-Host ' [*] cpf (Copy Path via fzf) - Interactive Path Selector  ' -ForegroundColor Cyan
        Write-Host '==========================================================' -ForegroundColor DarkGray
        Write-Host ''
        Write-Host 'Keyboard Shortcuts inside fzf:' -ForegroundColor Yellow
        Write-Host '----------------------------------------------------------' -ForegroundColor DarkGray
        Write-Host '  [Enter]       : Copy selected relative path to Clipboard and exit' -ForegroundColor Green
        Write-Host '  [Esc]         : Cancel without modifying clipboard' -ForegroundColor Green
        Write-Host '  [Ctrl + C]    : Abort selection immediately' -ForegroundColor Green
        Write-Host '  [Up / Down]   : Move selection cursor up / down' -ForegroundColor Green
        Write-Host '  [Tab]         : In terminal, auto-complete folder names for cpf' -ForegroundColor Green
        Write-Host ''
        Write-Host 'Usage & Examples:' -ForegroundColor Yellow
        Write-Host '----------------------------------------------------------' -ForegroundColor DarkGray
        Write-Host '  cpf                 : Interactively search & copy all files/folders from current directory' -ForegroundColor White
        Write-Host '  cpf <folder>        : Scope fuzzy finder to a specific folder (e.g. cpf docs)' -ForegroundColor White
        Write-Host '  cpf <folder><Tab>   : Tab-complete any existing folder name in terminal' -ForegroundColor White
        Write-Host '  cpf -d              : List and select directories/folders only' -ForegroundColor White
        Write-Host '  cpf -h / cpf --help : Display this shortcut and usage guide' -ForegroundColor White
        Write-Host ''
        return
    }

    # Resolve fzf executable
    $fzfCmd = Get-Command fzf.exe, fzf -ErrorAction SilentlyContinue | Select-Object -First 1
    $fzfExe = if ($fzfCmd) {
        $fzfCmd.Source
    } else {
        $fzfFallback = "$env:USERPROFILE\Documents\myenv\tools\fzf\fzf.exe"
        if (Test-Path $fzfFallback) { $fzfFallback } else { $null }
    }

    if (-not $fzfExe) {
        Write-Host "Error: 'fzf' executable was not found in PATH or in 'myenv\tools\fzf'." -ForegroundColor Red
        return
    }

    # Resolve root search location
    $currentLocation = (Get-Location).ProviderPath
    $searchTarget = if ([string]::IsNullOrWhiteSpace($Path) -or $Path -eq '.') {
        $currentLocation
    } else {
        $resolved = Resolve-Path -LiteralPath $Path -ErrorAction SilentlyContinue
        if ($resolved) { $resolved.ProviderPath } else { $currentLocation }
    }

    if (-not (Test-Path -LiteralPath $searchTarget)) {
        Write-Host "Error: Target path '$searchTarget' does not exist." -ForegroundColor Red
        return
    }

    # Stream relative files and folders into fzf
    try {
        $baseLen = $currentLocation.TrimEnd('\').Length
        $gciParams = @{
            LiteralPath = $searchTarget
            Recurse     = $true
            ErrorAction = 'SilentlyContinue'
        }
        if ($Directory) {
            $gciParams['Directory'] = $true
        }

        $itemsStream = Get-ChildItem @gciParams | ForEach-Object {
            $fullName = $_.FullName
            if ($fullName.StartsWith($currentLocation, [System.StringComparison]::OrdinalIgnoreCase)) {
                $rel = $fullName.Substring($baseLen).TrimStart('\', '/')
            } else {
                $baseUri = [System.Uri]($currentLocation.TrimEnd('\') + '\')
                $targetUri = [System.Uri]$fullName
                $rel = [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString())
            }
            $rel -replace '\\', '/' -replace '^\./', ''
        }

        if (-not $itemsStream) {
            Write-Host "No files or folders found to select in: $searchTarget" -ForegroundColor Yellow
            return
        }

        $fzfHeader = '[ENTER] Copy Path to Clipboard | [ESC] Cancel | Type to filter'
        $promptText = if ($Directory) { 'Copy Folder > ' } else { 'Copy Path > ' }

        $selected = $itemsStream | & $fzfExe --height=50% --layout=reverse --prompt="$promptText" --header="$fzfHeader" --preview-window=hidden
    } catch {
        Write-Host "Error running fzf: $_" -ForegroundColor Red
        return
    }

    # Clean exit if user pressed ESC or cancelled
    if ([string]::IsNullOrWhiteSpace($selected)) {
        return
    }

    $formattedPath = $selected.Trim() -replace '\\', '/' -replace '^\./', ''

    # Copy to clipboard and confirm
    try {
        $formattedPath | Set-Clipboard
        Write-Host "Copied to clipboard: $formattedPath" -ForegroundColor Green
    } catch {
        try {
            $formattedPath | clip.exe
            Write-Host "Copied to clipboard: $formattedPath" -ForegroundColor Green
        } catch {
            Write-Host "Failed to copy to clipboard: $_" -ForegroundColor Red
        }
    }
}

# Auto-complete folder names when typing 'cpf <Tab>'
Register-ArgumentCompleter -CommandName 'cpf' -ParameterName 'Path' -ScriptBlock {
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
    $pattern = if ([string]::IsNullOrWhiteSpace($wordToComplete)) { '*' } else { "$wordToComplete*" }
    Get-ChildItem -Path $pattern -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        $rel = (Resolve-Path -Relative -LiteralPath $_.FullName) -replace '^\.\\', '' -replace '\\', '/'
        [System.Management.Automation.CompletionResult]::new($rel, $rel, 'ProviderContainer', $_.Name)
    }
}