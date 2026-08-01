<#
.SYNOPSIS
    Fast Screen Region OCR & Translation Tool for MyEnv (Win+Shift+Q)
.DESCRIPTION
    Launches QuickTranslate tool to select a screen region, extract text via WinRT OCR,
    translate to Arabic, and present in a floating dark-mode UI.
#>

$exePath = Join-Path $PSScriptRoot "quick-translate\QuickTranslate.exe"
if (Test-Path $exePath) {
    Start-Process -FilePath $exePath
} else {
    Write-Error "QuickTranslate.exe not found at $exePath"
}
