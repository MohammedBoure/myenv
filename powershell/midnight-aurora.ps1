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

# Copy Path via fzf - Hierarchical Directory Navigator & Multi-Path Selector
function cpf {
    <#
    .SYNOPSIS
        Interactively browse directory hierarchy, select files/folders with arrow keys & Tab, and copy paths to clipboard.
    .DESCRIPTION
        Interactive folder tree navigator:
        - Folders start closed. Navigate with Up/Down (arrow keys).
        - Open/drill into folders with Right Arrow (->) or Enter.
        - Go back up with Left Arrow (<-) or selecting '[DIR] ../'.
        - Multi-select files and folders with Tab.
        - Copy paths to clipboard on Enter (for files) or Alt+Enter / Ctrl+Y (for folders/selections).
        - Automatically respects .gitignore and powershell/cpf-ignore.txt.
    .PARAMETER Path
        Optional initial directory to start browsing from. Defaults to current directory ('.').
    .PARAMETER Recurse
        Perform recursive flat search instead of hierarchical drill-down navigation.
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

        [Alias('r', 'Flat')]
        [switch]$Recurse,

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
        Write-Host '=====================================================================' -ForegroundColor DarkGray
        Write-Host ' [*] cpf (Copy Path) - Interactive Tree Navigator & Selector Guide   ' -ForegroundColor Cyan
        Write-Host '=====================================================================' -ForegroundColor DarkGray
        Write-Host ''
        Write-Host 'Interactive Navigation Controls:' -ForegroundColor Yellow
        Write-Host '---------------------------------------------------------------------' -ForegroundColor DarkGray
        Write-Host '  [->] / [Enter on Folder] : Open and expand directory' -ForegroundColor Green
        Write-Host '  [<-] / [Select ../]      : Go back to parent directory' -ForegroundColor Green
        Write-Host '  [Tab] / [Shift+Tab]      : Select / Deselect multiple files or folders' -ForegroundColor Green
        Write-Host '  [Enter on File]          : Copy file path(s) to Clipboard and exit' -ForegroundColor Green
        Write-Host '  [Alt + Enter] / [Ctrl+Y] : Copy highlighted folder / selected items immediately' -ForegroundColor Green
        Write-Host '  [Ctrl + A]               : Select all visible items in current directory' -ForegroundColor Green
        Write-Host '  [Ctrl + D]               : Deselect all selected items' -ForegroundColor Green
        Write-Host '  [Esc] / [Ctrl + C]       : Cancel and exit without modifying clipboard' -ForegroundColor Green
        Write-Host '  [Up / Down]              : Move selection cursor up / down' -ForegroundColor Green
        Write-Host ''
        Write-Host 'Parameters & Formatting Flags:' -ForegroundColor Yellow
        Write-Host '---------------------------------------------------------------------' -ForegroundColor DarkGray
        Write-Host '  -r, -Recurse             : Run full recursive flat search mode' -ForegroundColor White
        Write-Host '  -abs, -Absolute          : Copy full absolute paths (e.g. C:/Users/...)' -ForegroundColor White
        Write-Host '  -n, -Name                : Copy file/folder names only without paths' -ForegroundColor White
        Write-Host '  -md, -Markdown           : Copy as Markdown links [name](file:///path)' -ForegroundColor White
        Write-Host '  -s, -Space               : Join multiple paths with spaces (e.g. for git add)' -ForegroundColor White
        Write-Host '  -c, -Comma               : Join multiple paths with commas' -ForegroundColor White
        Write-Host '  -h, -Help                : Display this shortcuts and usage guide' -ForegroundColor White
        Write-Host ''
        Write-Host 'Ignore Config (.gitignore / cpf-ignore.txt):' -ForegroundColor Yellow
        Write-Host '---------------------------------------------------------------------' -ForegroundColor DarkGray
        Write-Host '  Central rules file       : %USERPROFILE%\Documents\myenv\powershell\cpf-ignore.txt' -ForegroundColor DarkCyan
        Write-Host '  Local rules              : Automatically inherits from .gitignore and .cpfignore' -ForegroundColor DarkCyan
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

    # Resolve root start location safely
    $currentLocation = (Get-Location).ProviderPath
    $searchTarget = $currentLocation

    if (-not [string]::IsNullOrWhiteSpace($Path) -and $Path -ne '.') {
        $cleanPath = $Path.Trim().TrimEnd('\', '/')
        $resolved = $null

        if (Test-Path -LiteralPath $cleanPath) {
            $resolved = (Resolve-Path -LiteralPath $cleanPath -ErrorAction SilentlyContinue).ProviderPath
        } elseif (Test-Path -Path $cleanPath) {
            $resolved = (Resolve-Path -Path $cleanPath -ErrorAction SilentlyContinue).ProviderPath
        }

        if (-not $resolved) {
            Write-Host "Error: Path '$Path' was not found in '$currentLocation'." -ForegroundColor Red
            return
        }
        $searchTarget = $resolved
    }

    # Load ignore rules (Global cpf-ignore.txt + local .gitignore / .cpfignore)
    $exactIgnoreNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $wildcardIgnores = [System.Collections.Generic.List[string]]::new()

    $ignoreFilesToLoad = [System.Collections.Generic.List[string]]::new()
    $globalIgnorePath = Join-Path $env:USERPROFILE "Documents\myenv\powershell\cpf-ignore.txt"
    if (Test-Path -LiteralPath $globalIgnorePath) { $ignoreFilesToLoad.Add($globalIgnorePath) }

    $localGitIgnore = Join-Path $searchTarget ".gitignore"
    if (Test-Path -LiteralPath $localGitIgnore) { $ignoreFilesToLoad.Add($localGitIgnore) }

    $localCpfIgnore = Join-Path $searchTarget ".cpfignore"
    if (Test-Path -LiteralPath $localCpfIgnore) { $ignoreFilesToLoad.Add($localCpfIgnore) }

    foreach ($igFile in $ignoreFilesToLoad) {
        try {
            $lines = Get-Content -LiteralPath $igFile -ErrorAction SilentlyContinue
            foreach ($line in $lines) {
                $trimmed = $line.Trim().TrimEnd('/', '\')
                if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith('#')) { continue }
                if ($trimmed.Contains('*') -or $trimmed.Contains('?')) {
                    $wildcardIgnores.Add($trimmed)
                } else {
                    [void]$exactIgnoreNames.Add($trimmed)
                }
            }
        } catch {}
    }

    # Always ensure critical root system folders are ignored
    @('.git', 'node_modules', 'AppData') | ForEach-Object { [void]$exactIgnoreNames.Add($_) }

    # Helper function to check if item should be ignored
    $isIgnored = {
        param([string]$name)
        if ($exactIgnoreNames.Contains($name)) { return $true }
        foreach ($pat in $wildcardIgnores) {
            if ($name -like $pat) { return $true }
        }
        return $false
    }

    # Helper function to format and copy paths to clipboard
    $copyAndExit = {
        param([string[]]$rawItems, [string]$currentDir)

        $formattedPaths = [System.Collections.Generic.List[string]]::new()

        foreach ($raw in $rawItems) {
            if ([string]::IsNullOrWhiteSpace($raw)) { continue }
            $cleaned = $raw -replace '^\s*\[DIR\]\s*|^\s*\[FILE\]\s*', '' -replace '/$', ''
            if ($cleaned -eq '..' -or [string]::IsNullOrWhiteSpace($cleaned)) { continue }

            $fullItemPath = Join-Path $currentDir $cleaned

            if ($Absolute) {
                $formattedPaths.Add(($fullItemPath -replace '\\', '/'))
            } elseif ($Name) {
                $formattedPaths.Add([System.IO.Path]::GetFileName($cleaned))
            } elseif ($Markdown) {
                $fullNormalized = $fullItemPath -replace '\\', '/'
                $baseName = [System.IO.Path]::GetFileName($cleaned)
                $formattedPaths.Add("[$baseName](file:///$fullNormalized)")
            } else {
                $baseLen = $currentLocation.TrimEnd('\', '/').Length
                if ($fullItemPath.StartsWith($currentLocation, [System.StringComparison]::OrdinalIgnoreCase)) {
                    $rel = $fullItemPath.Substring($baseLen).TrimStart('\', '/')
                } else {
                    $rel = $fullItemPath
                }
                $formattedPaths.Add(($rel -replace '\\', '/'))
            }
        }

        if ($formattedPaths.Count -eq 0) { return }

        $delimiter = if ($Space) { ' ' } elseif ($Comma) { ', ' } else { "`r`n" }
        $resultText = ($formattedPaths -join $delimiter)

        try {
            $resultText | Set-Clipboard
        } catch {
            try { $resultText | clip.exe } catch {
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

    # =========================================================================
    # RECURSIVE FLAT SEARCH MODE (-r / -Recurse)
    # =========================================================================
    if ($Recurse) {
        $baseLen = $currentLocation.TrimEnd('\', '/').Length
        $queue = [System.Collections.Generic.Queue[string]]::new()
        $queue.Enqueue($searchTarget)
        $itemsList = [System.Collections.Generic.List[string]]::new()

        while ($queue.Count -gt 0) {
            $curr = $queue.Dequeue()
            try {
                $entries = [System.IO.DirectoryInfo]::new($curr).GetFileSystemInfos()
            } catch { continue }

            foreach ($entry in $entries) {
                if (& $isIgnored $entry.Name) { continue }
                $isDir = ($entry.Attributes -band [System.IO.FileAttributes]::Directory) -ne 0
                if ($isDir) { $queue.Enqueue($entry.FullName) }

                $full = $entry.FullName
                if ($full.StartsWith($currentLocation, [System.StringComparison]::OrdinalIgnoreCase)) {
                    $rel = $full.Substring($baseLen).TrimStart('\', '/')
                } else {
                    $rel = $full
                }
                $prefix = if ($isDir) { '[DIR]  ' } else { '[FILE] ' }
                $suffix = if ($isDir) { '/' } else { '' }
                $itemsList.Add("$prefix$($rel -replace '\\', '/')$suffix")
            }
        }

        if ($itemsList.Count -eq 0) {
            Write-Host "No files or folders found in: $searchTarget" -ForegroundColor Yellow
            return
        }

        $fzfHeader = '[TAB] Select | [CTRL+A] All | [CTRL+D] None | [ENTER] Copy and Exit'
        $selected = $itemsList | & $fzfExe --multi `
            --height=50% `
            --layout=reverse `
            --prompt="Copy Path > " `
            --header="$fzfHeader" `
            --bind="ctrl-a:select-all,ctrl-d:deselect-all"

        if ($selected) {
            & $copyAndExit @($selected) $currentLocation
        }
        return
    }

    # =========================================================================
    # INTERACTIVE HIERARCHICAL DRILL-DOWN NAVIGATOR (Default)
    # =========================================================================
    $browsingDir = $searchTarget

    while ($true) {
        $dirEntries = [System.Collections.Generic.List[string]]::new()

        # Add parent folder option if not at drive root
        $parent = Split-Path $browsingDir -Parent
        if (-not [string]::IsNullOrWhiteSpace($parent)) {
            $dirEntries.Add('[DIR]  ../')
        }

        # List subdirectories (folders) first
        try {
            $dirs = Get-ChildItem -LiteralPath $browsingDir -Directory -ErrorAction SilentlyContinue |
                Where-Object { -not (& $isIgnored $_.Name) } |
                Sort-Object Name

            foreach ($d in $dirs) {
                $dirEntries.Add("[DIR]  $($d.Name)/")
            }
        } catch {}

        # List files next
        try {
            $files = Get-ChildItem -LiteralPath $browsingDir -File -ErrorAction SilentlyContinue |
                Where-Object { -not (& $isIgnored $_.Name) } |
                Sort-Object Name

            foreach ($f in $files) {
                $dirEntries.Add("[FILE] $($f.Name)")
            }
        } catch {}

        if ($dirEntries.Count -eq 0) {
            Write-Host "Folder is empty: $browsingDir" -ForegroundColor Yellow
            if ($parent) {
                $browsingDir = $parent
                continue
            } else {
                return
            }
        }

        # Calculate display path for prompt
        $baseLen = $currentLocation.TrimEnd('\', '/').Length
        $relDisplay = if ($browsingDir.StartsWith($currentLocation, [System.StringComparison]::OrdinalIgnoreCase)) {
            $rel = $browsingDir.Substring($baseLen).TrimStart('\', '/') -replace '\\', '/'
            if ([string]::IsNullOrWhiteSpace($rel)) { '.' } else { $rel }
        } else {
            $browsingDir -replace '\\', '/'
        }

        $fzfHeader = '[->/ENTER] Open Folder | [<-] Back | [TAB] Select | [ENTER on File / ALT+ENTER] Copy'
        $promptText = "Folder: $relDisplay > "

        $fzfOutput = $dirEntries | & $fzfExe --multi `
            --height=50% `
            --layout=reverse `
            --prompt="$promptText" `
            --header="$fzfHeader" `
            --expect=right,left,alt-enter,ctrl-y `
            --bind="ctrl-a:select-all,ctrl-d:deselect-all,left:accept,right:accept"

        # User pressed Esc or cancelled
        if ($null -eq $fzfOutput -or $fzfOutput.Count -eq 0) {
            return
        }

        $lines = @($fzfOutput)
        $key = $lines[0]
        $selectedItems = if ($lines.Count -gt 1) { $lines[1..($lines.Count - 1)] } else { @() }

        # Handle Left Arrow Key (Go to Parent)
        if ($key -eq 'left') {
            if (-not [string]::IsNullOrWhiteSpace($parent)) {
                $browsingDir = $parent
            }
            continue
        }

        if ($selectedItems.Count -eq 0) {
            continue
        }

        # If user explicitly pressed Alt+Enter or Ctrl+Y to copy highlighted folder or multi-selected items
        if ($key -eq 'alt-enter' -or $key -eq 'ctrl-y') {
            & $copyAndExit $selectedItems $browsingDir
            return
        }

        # If multiple items were selected with Tab, copy them and exit
        if ($selectedItems.Count -gt 1) {
            & $copyAndExit $selectedItems $browsingDir
            return
        }

        # Single item selected
        $singleItem = $selectedItems[0]

        # Selected '[DIR]  ../' -> Go up
        if ($singleItem -match '^\s*\[DIR\]\s*\.\./') {
            if (-not [string]::IsNullOrWhiteSpace($parent)) {
                $browsingDir = $parent
            }
            continue
        }

        # Selected a folder
        if ($singleItem.StartsWith('[DIR]')) {
            $folderName = $singleItem -replace '^\s*\[DIR\]\s*', '' -replace '/$', ''
            $targetSubDir = Join-Path $browsingDir $folderName

            # If Right Arrow or Enter was pressed, drill into folder
            if ($key -eq 'right' -or [string]::IsNullOrWhiteSpace($key)) {
                if (Test-Path -LiteralPath $targetSubDir -PathType Container) {
                    $browsingDir = $targetSubDir
                    continue
                }
            }

            # Otherwise copy folder path
            & $copyAndExit @($singleItem) $browsingDir
            return
        }

        # Selected a file -> Copy path and exit!
        if ($singleItem.StartsWith('[FILE]')) {
            & $copyAndExit @($singleItem) $browsingDir
            return
        }
    }
}

# Auto-complete folder names when typing 'cpf <Tab>'
Register-ArgumentCompleter -CommandName 'cpf' -ParameterName 'Path' -ScriptBlock {
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
    $cleanWord = if ([string]::IsNullOrWhiteSpace($wordToComplete)) { '' } else { $wordToComplete.Replace('/', '\') }
    $searchDir = if ($cleanWord.Contains('\')) {
        $parent = [System.IO.Path]::GetDirectoryName($cleanWord)
        if ([string]::IsNullOrWhiteSpace($parent)) { '.' } else { $parent }
    } else { '.' }

    $prefix = if ($cleanWord.Contains('\')) { [System.IO.Path]::GetFileName($cleanWord) } else { $cleanWord }

    if (Test-Path -LiteralPath $searchDir) {
        Get-ChildItem -LiteralPath $searchDir -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like "$prefix*" -and $_.Name -notmatch '^\.(git|gemini|vscode|idea)$|node_modules|AppData' } |
            ForEach-Object {
                $rel = (Resolve-Path -Relative -LiteralPath $_.FullName) -replace '^\.\\', '' -replace '\\', '/'
                [System.Management.Automation.CompletionResult]::new($rel, $rel, 'ProviderContainer', $_.Name)
            }
    }
}