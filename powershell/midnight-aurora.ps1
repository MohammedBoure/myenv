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
# Copy Path via fzf - Interactively select single or multiple files/folders and copy normalized relative/absolute paths
function cpf {
    <#
    .SYNOPSIS
        Interactively search and select file(s)/folder(s) with fzf, then copy normalized paths to the clipboard.
    .DESCRIPTION
        Launches fzf with multi-selection enabled to search and select files or folders starting from the working directory.
        Supports Tab multi-selection, Select All (Alt+A), Deselect All (Alt+D), and interactive Preview (Ctrl+P).
        Formats paths as relative (default), absolute (-abs), filename only (-n), or Markdown links (-md),
        and copies them to the clipboard separated by newlines (default), spaces (-s), or commas (-c).
    .PARAMETER Path
        Optional root folder to search from. Supports Tab completion. Defaults to current directory ('.').
    .PARAMETER Directory
        Search and select folders/directories only.
    .PARAMETER Absolute
        Copy full absolute paths (e.g. C:/Users/...).
    .PARAMETER Name
        Copy base file/folder names only without directory paths.
    .PARAMETER Markdown
        Copy as Markdown links [name](file:///path).
    .PARAMETER Space
        Join multiple selected paths with spaces instead of newlines.
    .PARAMETER Comma
        Join multiple selected paths with commas.
    .PARAMETER Help
        Display the shortcuts and usage guide.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory = $false)]
        [string]$Path = '.',

        [Alias('d', 'FoldersOnly')]
        [switch]$Directory,

        [Alias('abs', 'Full')]
        [switch]$Absolute,

        [Alias('n', 'Base')]
        [switch]$Name,

        [Alias('md', 'Link')]
        [switch]$Markdown,

        [Alias('s')]
        [switch]$Space,

        [Alias('c')]
        [switch]$Comma,

        [Alias('h', '?')]
        [switch]$Help
    )

    # Display Help & Shortcuts Guide if requested
    if ($Help -or ($Path -in @('-h', '--help', '-?', 'help', '/?', '/h'))) {
        Write-Host ''
        Write-Host '==========================================================' -ForegroundColor DarkGray
        Write-Host ' [*] cpf (Copy Path via fzf) - Multi-Path Selector Guide  ' -ForegroundColor Cyan
        Write-Host '==========================================================' -ForegroundColor DarkGray
        Write-Host ''
        Write-Host 'Keyboard Shortcuts inside fzf:' -ForegroundColor Yellow
        Write-Host '----------------------------------------------------------' -ForegroundColor DarkGray
        Write-Host '  [Tab] / [Shift+Tab] : Select / Deselect multiple files or folders' -ForegroundColor Green
        Write-Host '  [Alt + A]           : Select all visible items' -ForegroundColor Green
        Write-Host '  [Alt + D]           : Deselect all selected items' -ForegroundColor Green
        Write-Host '  [Ctrl + P]          : Toggle interactive file/folder preview' -ForegroundColor Green
        Write-Host '  [Enter]             : Copy selected path(s) to Clipboard and exit' -ForegroundColor Green
        Write-Host '  [Esc] / [Ctrl + C]  : Cancel without modifying clipboard' -ForegroundColor Green
        Write-Host '  [Up / Down]         : Move selection cursor up / down' -ForegroundColor Green
        Write-Host ''
        Write-Host 'Parameters & Formatting Flags:' -ForegroundColor Yellow
        Write-Host '----------------------------------------------------------' -ForegroundColor DarkGray
        Write-Host '  -d, -Directory      : List and select directories/folders only' -ForegroundColor White
        Write-Host '  -abs, -Absolute     : Copy full absolute paths (e.g. C:/Users/...)' -ForegroundColor White
        Write-Host '  -n, -Name           : Copy file/folder names only without paths' -ForegroundColor White
        Write-Host '  -md, -Markdown      : Copy as Markdown links [name](file:///path)' -ForegroundColor White
        Write-Host '  -s, -Space          : Join multiple paths with spaces (e.g. for git add)' -ForegroundColor White
        Write-Host '  -c, -Comma          : Join multiple paths with commas' -ForegroundColor White
        Write-Host '  -h, -Help           : Display this shortcuts and usage guide' -ForegroundColor White
        Write-Host ''
        Write-Host 'Usage & Examples:' -ForegroundColor Yellow
        Write-Host '----------------------------------------------------------' -ForegroundColor DarkGray
        Write-Host '  cpf                 : Multi-select files/folders (Tab to pick multiple)' -ForegroundColor White
        Write-Host '  cpf scripts         : Scope search to scripts folder' -ForegroundColor White
        Write-Host '  cpf -d              : Select and copy folders only' -ForegroundColor White
        Write-Host '  cpf -s              : Copy multiple paths separated by spaces' -ForegroundColor White
        Write-Host '  cpf -abs            : Copy absolute system paths' -ForegroundColor White
        Write-Host '  cpf -md             : Copy as Markdown links' -ForegroundColor White
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

        $fzfHeader = '[TAB] Select | [ALT+A] All | [ALT+D] None | [CTRL+P] Preview | [ENTER] Copy'
        $promptText = if ($Directory) { 'Copy Folder(s) > ' } else { 'Copy Path(s) > ' }

        $selected = $itemsStream | & $fzfExe --multi `
            --height=50% `
            --layout=reverse `
            --prompt="$promptText" `
            --header="$fzfHeader" `
            --preview='cmd /c if exist "{}" (if exist "{}\*" (dir /b /o:n "{}") else (type "{}"))' `
            --preview-window="right:50%:hidden:wrap" `
            --bind="alt-a:select-all,alt-d:deselect-all,ctrl-p:toggle-preview"
    } catch {
        Write-Host "Error running fzf: $_" -ForegroundColor Red
        return
    }

    # Clean exit if user pressed ESC or cancelled
    if ($null -eq $selected -or $selected.Count -eq 0) {
        return
    }

    $selectedItems = @($selected) | ForEach-Object {
        if ($_ -is [string]) {
            $_.Split("`r`n", [System.StringSplitOptions]::RemoveEmptyEntries)
        } else {
            $_
        }
    } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    if ($selectedItems.Count -eq 0) {
        return
    }

    $formattedPaths = [System.Collections.Generic.List[string]]::new()

    foreach ($item in $selectedItems) {
        $cleanItem = $item.Trim()
        if ([string]::IsNullOrWhiteSpace($cleanItem)) { continue }

        $normalizedRel = $cleanItem -replace '\\', '/' -replace '^\./', ''

        if ($Absolute) {
            $fullPath = [System.IO.Path]::GetFullPath((Join-Path $currentLocation $cleanItem)) -replace '\\', '/'
            $formattedPaths.Add($fullPath)
        } elseif ($Name) {
            $fileName = [System.IO.Path]::GetFileName($cleanItem.TrimEnd('\', '/'))
            $formattedPaths.Add($fileName)
        } elseif ($Markdown) {
            $fullPath = [System.IO.Path]::GetFullPath((Join-Path $currentLocation $cleanItem)) -replace '\\', '/'
            $fileName = [System.IO.Path]::GetFileName($cleanItem.TrimEnd('\', '/'))
            $formattedPaths.Add("[$fileName](file:///$fullPath)")
        } else {
            $formattedPaths.Add($normalizedRel)
        }
    }

    if ($formattedPaths.Count -eq 0) {
        return
    }

    $delimiter = if ($Space) {
        ' '
    } elseif ($Comma) {
        ', '
    } else {
        "`r`n"
    }

    $resultText = ($formattedPaths -join $delimiter)

    # Copy to clipboard and confirm
    try {
        $resultText | Set-Clipboard
    } catch {
        try {
            $resultText | clip.exe
        } catch {
            Write-Host "Failed to copy to clipboard: $_" -ForegroundColor Red
            return
        }
    }

    if ($formattedPaths.Count -eq 1) {
        Write-Host "Copied 1 path to clipboard: $($formattedPaths[0])" -ForegroundColor Green
    } else {
        Write-Host "Copied $($formattedPaths.Count) paths to clipboard:" -ForegroundColor Green
        $previewCount = [System.Math]::Min($formattedPaths.Count, 8)
        for ($i = 0; $i -lt $previewCount; $i++) {
            Write-Host "  $($i + 1). $($formattedPaths[$i])" -ForegroundColor DarkCyan
        }
        if ($formattedPaths.Count -gt 8) {
            Write-Host "  ... and $($formattedPaths.Count - 8) more" -ForegroundColor DarkGray
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