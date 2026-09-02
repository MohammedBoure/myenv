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

    if (-not [Console]::IsInputRedirected -and -not [Console]::IsOutputRedirected) {
        Set-PSReadLineOption -EditMode Windows
        Set-PSReadLineOption -PredictionSource HistoryAndPlugin
        Set-PSReadLineOption -PredictionViewStyle InlineView
        Set-PSReadLineOption -BellStyle None
        Set-PSReadLineKeyHandler -Key Tab -Function MenuComplete
        Set-PSReadLineKeyHandler -Key Ctrl+Backspace -Function BackwardKillWord
        Set-PSReadLineKeyHandler -Key Ctrl+v -Function Paste
        Set-PSReadLineKeyHandler -Key Ctrl+c -Function CopyOrCancelLine
        Set-PSReadLineKeyHandler -Key UpArrow -Function HistorySearchBackward
        Set-PSReadLineKeyHandler -Key DownArrow -Function HistorySearchForward
        Set-PSReadLineKeyHandler -Key Ctrl+f -Function ForwardChar
        Set-PSReadLineKeyHandler -Chord 'Ctrl+RightArrow' -Function NextWord

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
            Command                     = 'Cyan'
            Parameter                   = 'Yellow'
            String                      = 'Green'
            Operator                    = 'DarkCyan'
            Variable                    = 'Magenta'
            Comment                     = 'DarkGray'
            Keyword                     = 'Blue'
            Type                        = 'DarkYellow'
            Number                      = 'DarkGreen'
            Member                      = 'White'
            InlinePrediction            = 'DarkGray'
            ListPredictionColor         = 'Yellow'
            ListPredictionSelectedColor = "`e[48;5;238m"
            ListPredictionTooltipColor  = "`e[97;2;3m"
        }
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

# Copy Path via fzf - VS Code Style Interactive Tree Explorer & Multi-Path Selector
function cpf {
    <#
    .SYNOPSIS
        VS Code Explorer in Terminal: Browse folder tree in-place, expand/collapse with Left/Right arrows, and copy paths.
    .DESCRIPTION
        Interactive tree explorer like VS Code:
        - Folders start closed (> folder/). Navigate with Up/Down arrows.
        - Press Right Arrow (->) or Enter to expand a folder in-place (v folder/).
        - Press Left Arrow (<-) to collapse an open folder or its parent.
        - Press Enter on any file to copy its relative path to Clipboard and exit.
        - Press Alt+Enter or Ctrl+Y on any folder/file to copy its path immediately.
        - Use Tab for multi-selection across any branches, then Enter to copy all.
        - Automatically respects .gitignore and powershell/cpf-ignore.txt.
    .PARAMETER Path
        Optional directory to start tree exploration from. Defaults to current directory ('.').
    .PARAMETER Recurse
        Perform recursive flat search instead of in-place tree exploration.
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
        Write-Host ' [*] cpf (Copy Path) - VS Code Terminal Tree Explorer Guide          ' -ForegroundColor Cyan
        Write-Host '=====================================================================' -ForegroundColor DarkGray
        Write-Host ''
        Write-Host 'Interactive Tree Controls (VS Code Style):' -ForegroundColor Yellow
        Write-Host '---------------------------------------------------------------------' -ForegroundColor DarkGray
        Write-Host '  [->]                     : Expand folder in-place' -ForegroundColor Green
        Write-Host '  [<-]                     : Collapse folder / parent folder in-place' -ForegroundColor Green
        Write-Host '  [Tab] / [Shift+Tab]      : Select / Deselect multiple files or folders' -ForegroundColor Green
        Write-Host '  [Enter]                  : Copy selected file or folder path(s) to Clipboard and exit' -ForegroundColor Green
        Write-Host '  [Alt + Enter] / [Ctrl+Y] : Copy highlighted item immediately and exit' -ForegroundColor Green
        Write-Host '  [Ctrl + A]               : Select all visible items' -ForegroundColor Green
        Write-Host '  [Ctrl + D]               : Deselect all selected items' -ForegroundColor Green
        Write-Host '  [Esc] / [Ctrl + C]       : Cancel without modifying clipboard' -ForegroundColor Green
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
        param([string[]]$selectedRelPaths)

        $formattedPaths = [System.Collections.Generic.List[string]]::new()

        foreach ($rawRel in $selectedRelPaths) {
            if ([string]::IsNullOrWhiteSpace($rawRel)) { continue }
            $cleanRel = $rawRel.Trim().TrimEnd('/', '\')
            if ([string]::IsNullOrWhiteSpace($cleanRel)) { continue }

            $fullItemPath = Join-Path $searchTarget $cleanRel

            if ($Absolute) {
                $formattedPaths.Add(($fullItemPath -replace '\\', '/'))
            } elseif ($Name) {
                $formattedPaths.Add([System.IO.Path]::GetFileName($cleanRel))
            } elseif ($Markdown) {
                $fullNormalized = $fullItemPath -replace '\\', '/'
                $baseName = [System.IO.Path]::GetFileName($cleanRel)
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
                $prefix = if ($isDir) { '> ' } else { '  ' }
                $suffix = if ($isDir) { '/' } else { '' }
                $itemsList.Add("$rel`t$prefix$($rel -replace '\\', '/')$suffix")
            }
        }

        if ($itemsList.Count -eq 0) {
            Write-Host "No files or folders found in: $searchTarget" -ForegroundColor Yellow
            return
        }

        $fzfHeader = '[TAB] Select | [CTRL+A] All | [CTRL+D] None | [ENTER] Copy and Exit'
        $selected = $itemsList | & $fzfExe --multi `
            --height=60% `
            --layout=reverse `
            --prompt="Copy Path > " `
            --header="$fzfHeader" `
            --delimiter="`t" `
            --with-nth=2 `
            --bind="ctrl-a:select-all,ctrl-d:deselect-all"

        if ($selected) {
            $pickedRels = [System.Collections.Generic.List[string]]::new()
            foreach ($item in @($selected)) {
                $p = "$item".Split("`t")[0]
                if (-not [string]::IsNullOrWhiteSpace($p)) { $pickedRels.Add($p) }
            }
            & $copyAndExit $pickedRels
        }
        return
    }

    # =========================================================================
    # VS CODE STYLE IN-PLACE INTERACTIVE TREE EXPLORER (Default)
    # =========================================================================
    $expandedFolders = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $lastTargetRel = ''

    function Get-TreeRows($targetDir) {
        $collector = [System.Collections.Generic.List[string]]::new()

        $walk = {
            param($dir, $depth, $baseRel)
            $indent = '    ' * $depth

            # Subdirectories first
            try {
                $dirs = Get-ChildItem -LiteralPath $dir -Directory -ErrorAction SilentlyContinue |
                    Where-Object { -not (& $isIgnored $_.Name) } |
                    Sort-Object Name

                foreach ($d in $dirs) {
                    $rel = if ($baseRel) { "$baseRel/$($d.Name)" } else { $d.Name }
                    $isExp = $expandedFolders.Contains($rel)
                    $icon = if ($isExp) { 'v ' } else { '> ' }
                    $collector.Add("$rel`t$indent$icon$($d.Name)/")

                    if ($isExp) {
                        & $walk $d.FullName ($depth + 1) $rel
                    }
                }
            } catch {}

            # Files next
            try {
                $files = Get-ChildItem -LiteralPath $dir -File -ErrorAction SilentlyContinue |
                    Where-Object { -not (& $isIgnored $_.Name) } |
                    Sort-Object Name

                foreach ($f in $files) {
                    $rel = if ($baseRel) { "$baseRel/$($f.Name)" } else { $f.Name }
                    $collector.Add("$rel`t$indent  $($f.Name)")
                }
            } catch {}
        }

        & $walk $targetDir 0 ''
        return $collector
    }

    while ($true) {
        $treeList = Get-TreeRows $searchTarget

        if ($treeList.Count -eq 0) {
            Write-Host "Folder is empty: $searchTarget" -ForegroundColor Yellow
            return
        }

        # Calculate exact cursor position to preserve motion location
        $posIndex = 1
        if (-not [string]::IsNullOrWhiteSpace($lastTargetRel)) {
            for ($i = 0; $i -lt $treeList.Count; $i++) {
                $rowStr = "$($treeList[$i])"
                if ($rowStr.Contains("`t")) {
                    $entryRel = $rowStr.Split("`t")[0]
                    if ($entryRel -ieq $lastTargetRel) {
                        $posIndex = $i + 1
                        break
                    }
                }
            }
        }

        # Prompt & Header
        $baseLen = $currentLocation.TrimEnd('\', '/').Length
        $relDisplay = if ($searchTarget.StartsWith($currentLocation, [System.StringComparison]::OrdinalIgnoreCase)) {
            $rel = $searchTarget.Substring($baseLen).TrimStart('\', '/') -replace '\\', '/'
            if ([string]::IsNullOrWhiteSpace($rel)) { '.' } else { $rel }
        } else {
            $searchTarget -replace '\\', '/'
        }

        $fzfHeader = '[->] Expand | [<-] Collapse | [TAB] Select | [ENTER] Copy & Exit'
        $promptText = "Tree Explorer: $relDisplay > "

        $fzfOutput = $treeList | & $fzfExe --multi `
            --height=60% `
            --layout=reverse `
            --prompt="$promptText" `
            --header="$fzfHeader" `
            --delimiter="`t" `
            --with-nth=2 `
            --expect=right,left,alt-enter,ctrl-y `
            --sync `
            --bind="start:pos($posIndex),ctrl-a:select-all,ctrl-d:deselect-all,left:accept,right:accept"

        # User pressed Esc or cancelled
        if ($null -eq $fzfOutput) {
            return
        }

        # Normalize fzf output lines safely
        $outputList = [System.Collections.Generic.List[string]]::new()
        if ($fzfOutput -is [string]) {
            $splitLines = $fzfOutput -split "`r?`n"
            foreach ($sl in $splitLines) {
                $outputList.Add("$sl")
            }
        } else {
            foreach ($entryItem in @($fzfOutput)) {
                if ($entryItem -is [string]) {
                    $splitLines = $entryItem -split "`r?`n"
                    foreach ($sl in $splitLines) {
                        $outputList.Add("$sl")
                    }
                } else {
                    $outputList.Add("$entryItem")
                }
            }
        }

        if ($outputList.Count -eq 0) {
            return
        }

        $key = ''
        $selectedLines = [System.Collections.Generic.List[string]]::new()

        if ($outputList.Count -ge 2) {
            $firstLine = "$($outputList[0])".Trim().ToLowerInvariant()
            if ($firstLine -in @('right', 'left', 'alt-enter', 'ctrl-y', '')) {
                $key = $firstLine
                for ($idx = 1; $idx -lt $outputList.Count; $idx++) {
                    $itemStr = "$($outputList[$idx])".Trim()
                    if (-not [string]::IsNullOrWhiteSpace($itemStr)) {
                        $selectedLines.Add($itemStr)
                    }
                }
            } else {
                foreach ($itemEntry in $outputList) {
                    $itemStr = "$itemEntry".Trim()
                    if (-not [string]::IsNullOrWhiteSpace($itemStr)) {
                        $selectedLines.Add($itemStr)
                    }
                }
            }
        } elseif ($outputList.Count -eq 1) {
            $singleStr = "$($outputList[0])".Trim()
            if ($singleStr -in @('right', 'left', 'alt-enter', 'ctrl-y')) {
                $key = $singleStr.ToLowerInvariant()
            } elseif (-not [string]::IsNullOrWhiteSpace($singleStr)) {
                $selectedLines.Add($singleStr)
            }
        }

        if ($selectedLines.Count -eq 0) {
            continue
        }

        # If user explicitly pressed Alt+Enter or Ctrl+Y to copy immediately
        if ($key -eq 'alt-enter' -or $key -eq 'ctrl-y') {
            $pickedRels = [System.Collections.Generic.List[string]]::new()
            foreach ($sl in $selectedLines) {
                $p = $sl.Split("`t")[0]
                if (-not [string]::IsNullOrWhiteSpace($p)) { $pickedRels.Add($p) }
            }
            & $copyAndExit $pickedRels
            return
        }

        # If multiple items were selected with Tab, copy all and exit
        if ($selectedLines.Count -gt 1) {
            $pickedRels = [System.Collections.Generic.List[string]]::new()
            foreach ($sl in $selectedLines) {
                $p = $sl.Split("`t")[0]
                if (-not [string]::IsNullOrWhiteSpace($p)) { $pickedRels.Add($p) }
            }
            & $copyAndExit $pickedRels
            return
        }

        # Single item selected
        $singleLine = "$($selectedLines[0])"
        $parts = $singleLine.Split("`t")
        $targetRel = $parts[0]
        $display = if ($parts.Count -gt 1) { $parts[1] } else { '' }
        $isFolder = $display.Trim().EndsWith('/') -or (Test-Path -LiteralPath (Join-Path $searchTarget $targetRel) -PathType Container)

        # Record position so cursor stays exactly on this item
        $lastTargetRel = $targetRel

        # Handle Left Arrow Key (Collapse folder or collapse parent)
        if ($key -eq 'left') {
            if ($isFolder -and $expandedFolders.Contains($targetRel)) {
                # Collapse this folder
                [void]$expandedFolders.Remove($targetRel)
                # Also remove any nested subfolders
                $subPrefix = "$targetRel/"
                $toRemove = @($expandedFolders | Where-Object { $_.StartsWith($subPrefix, [System.StringComparison]::OrdinalIgnoreCase) })
                foreach ($tr in $toRemove) { [void]$expandedFolders.Remove($tr) }
                $lastTargetRel = $targetRel
            } elseif ($targetRel.Contains('/')) {
                # Collapse parent folder
                $parentRel = [System.IO.Path]::GetDirectoryName($targetRel.Replace('/', '\')).Replace('\', '/')
                if (-not [string]::IsNullOrWhiteSpace($parentRel)) {
                    [void]$expandedFolders.Remove($parentRel)
                    $lastTargetRel = $parentRel
                }
            }
            continue
        }

        # Handle Right Arrow Key on a Folder (Expand in-place)
        if ($key -eq 'right') {
            if ($isFolder) {
                [void]$expandedFolders.Add($targetRel)
                $lastTargetRel = $targetRel
            }
            continue
        }

        # Enter Key -> Copy selected file or folder path and exit!
        & $copyAndExit @($targetRel)
        return
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