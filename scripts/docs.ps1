param(
    [string]$Topic = ""
)

$docsDir = Join-Path $env:USERPROFILE "Documents\myenv\docs"

function Show-Header {
    Write-Host "==========================================================" -ForegroundColor DarkGray
    Write-Host " MyEnv Documentation & Shortcut Guide (Bilingual / مزدوج) " -ForegroundColor Cyan
    Write-Host "==========================================================" -ForegroundColor DarkGray
}

function Show-GlazeWM {
    Show-Header
    Write-Host ""
    Write-Host "GlazeWM Tiling Window Manager Keybindings:" -ForegroundColor Yellow
    Write-Host "----------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host "  Focus Window      | Focus : Alt + H / J / K / L  or  Alt + Arrows" -ForegroundColor Green
    Write-Host "  Move Window       | Move  : Alt + Shift + H / J / K / L" -ForegroundColor Green
    Write-Host "  Focus Workspace   | Worksp: Alt + 1..8 (Left), Alt + 9..0 (Right)" -ForegroundColor Green
    Write-Host "  Move Workspace    | MoveWS: Alt + Shift + 1..0" -ForegroundColor Green
    Write-Host "  Split Direction   | Split : Alt + V (Toggle) | Alt+Shift+V (Vert) | Alt+Ctrl+V (Horiz)" -ForegroundColor Green
    Write-Host "  Window Modes      | Mode  : Alt + Space (Cycle) | Alt+Shift+Space (Float) | Alt+F (Full)" -ForegroundColor Green
    Write-Host "  Window Actions    | Action: Alt + Q (Close) | Alt + M (Minimize) | Alt + T (Tile)" -ForegroundColor Green
    Write-Host "  Resize Window     | Resize: Alt + R (Interactive) | Alt + U/P (Width) | Alt + I/O (Height)" -ForegroundColor Green
    Write-Host "  App Launcher      | Search: Alt + Shift + Q  (WPF Search Dialog)" -ForegroundColor Yellow
    Write-Host "  Quick Translate   | Trans : Win + Shift + C  (Instant Selected Text)" -ForegroundColor Yellow
    Write-Host "                    | OCR   : Win + Shift + Q  (Screen Region OCR)" -ForegroundColor Yellow
    Write-Host "  System Tools      | Tools : Alt + Shift + S  (Screenshot) | Alt + Shift + X (Task Manager)" -ForegroundColor Magenta
    Write-Host "                    |       : Alt + Shift + M  (Mute Audio) | Alt + Shift + Z (Transparency)" -ForegroundColor Magenta
    Write-Host "  WM Control        | Ctrl  : Alt + Shift + R  (Reload Config) | Alt + Shift + E (Exit)" -ForegroundColor DarkGray
}

function Show-Translate {
    Show-Header
    Write-Host ""
    Write-Host "QuickTranslate Shortcuts & Usage:" -ForegroundColor Yellow
    Write-Host "----------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host "  Win + Shift + C  | Alt+Shift+C : Instant Selected Text Translation" -ForegroundColor Green
    Write-Host "  Win + Shift + Q  | Alt+Shift+T : Drag-Select Screen Region OCR Translation" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Rebuild Command                : cd tools\quick-translate ; dotnet build -c Release" -ForegroundColor Cyan
}

function Show-CMD {
    Show-Header
    Write-Host ""
    Write-Host "CMD Aliases & Clink Features:" -ForegroundColor Yellow
    Write-Host "----------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host "  cd [path]        : Navigate to directory + auto-list files (Auto-LS)" -ForegroundColor Green
    Write-Host "  croot            : Jump to %USERPROFILE% + Auto-LS" -ForegroundColor Green
    Write-Host "  ls / ll / la     : File listing (Brief / Detailed / All incl. hidden)" -ForegroundColor Green
    Write-Host "  clear            : Clear console screen" -ForegroundColor Green
    Write-Host "  cb [command]     : Execute command and copy output to Clipboard" -ForegroundColor Green
    Write-Host "  sudo [command]   : Run command or new CMD as Administrator" -ForegroundColor Green
    Write-Host "  git aliases      : gs (status), ga (add), gc (commit), gp (push), gl (log)" -ForegroundColor Green
    Write-Host "  Clink Hotkeys    : Ctrl+Space / F7 (History Popup), Right Arrow (Accept suggestion)" -ForegroundColor Yellow
}

function Show-PowerShell {
    Show-Header
    Write-Host ""
    Write-Host "PowerShell Profile & Features:" -ForegroundColor Yellow
    Write-Host "----------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host "  cd [path]        : Navigate + Auto-LS (Get-ChildItem)" -ForegroundColor Green
    Write-Host "  cpf [path]       : Fuzzy find file via fzf & copy relative path to Clipboard" -ForegroundColor Green
    Write-Host "  cb [cmd] | cb    : Copy command output directly to Clipboard" -ForegroundColor Green
    Write-Host "  sudo [command]   : Run elevated command in current working directory" -ForegroundColor Green
    Write-Host "  PSReadLine       : Ctrl+Backspace (Delete word), Tab (Menu completion)" -ForegroundColor Yellow
    Write-Host "                   : Ctrl+R (Interactive History Search), Down/Up (History match)" -ForegroundColor Yellow
}

function Show-Scripts {
    Show-Header
    Write-Host ""
    Write-Host "Automation Scripts (myenv\scripts):" -ForegroundColor Yellow
    Write-Host "----------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host "  setup-all.ps1              : Master environment setup and re-linking script" -ForegroundColor Green
    Write-Host "  install-packages.ps1       : Restore Winget packages from winget-packages.json" -ForegroundColor Green
    Write-Host "  app-launcher.ps1           : WPF centered application launcher dialog" -ForegroundColor Green
    Write-Host "  install-clink.ps1          : Download and configure Clink for CMD" -ForegroundColor Green
    Write-Host "  set-taskbar-autohide.ps1   : Toggle Windows Taskbar autohide" -ForegroundColor Green
    Write-Host "  disable-alt-shift-lang.ps1 : Disable Alt+Shift, keep Win+Space for language" -ForegroundColor Green
}

function Show-Menu {
    Show-Header
    Write-Host ""
    Write-Host "Select an option:" -ForegroundColor White
    Write-Host " [1] GlazeWM Keybindings" -ForegroundColor Green
    Write-Host " [2] QuickTranslate OCR & Selection" -ForegroundColor Green
    Write-Host " [3] CMD Aliases & Clink" -ForegroundColor Green
    Write-Host " [4] PowerShell Profile & Shortcuts" -ForegroundColor Green
    Write-Host " [5] Automation Scripts List" -ForegroundColor Green
    Write-Host " [6] Open Full Docs Folder in Explorer" -ForegroundColor Yellow
    Write-Host " [Q] Exit" -ForegroundColor DarkGray
    Write-Host ""
    $choice = Read-Host "Enter option (1-6 or Q)"
    switch ($choice.Trim()) {
        "1" { Show-GlazeWM }
        "2" { Show-Translate }
        "3" { Show-CMD }
        "4" { Show-PowerShell }
        "5" { Show-Scripts }
        "6" { Invoke-Item $docsDir }
        default { return }
    }
}

$cleanTopic = if ($Topic) { $Topic.ToLower().Trim() } else { "" }

switch -Wildcard ($cleanTopic) {
    "*wm*"         { Show-GlazeWM }
    "*glaze*"      { Show-GlazeWM }
    "*key*"        { Show-GlazeWM }
    "*trans*"      { Show-Translate }
    "*ocr*"        { Show-Translate }
    "*cmd*"        { Show-CMD }
    "*alias*"      { Show-CMD }
    "*ps*"         { Show-PowerShell }
    "*power*"      { Show-PowerShell }
    "*script*"     { Show-Scripts }
    "*auto*"       { Show-Scripts }
    "*open*"       { Invoke-Item $docsDir }
    default        { Show-Menu }
}
