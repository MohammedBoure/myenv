<#
.SYNOPSIS
    Fast Screen Region OCR & Translation Tool for MyEnv (Win+Shift+X, Win+Shift+Q, Win+Shift+C)
.DESCRIPTION
    Launches QuickTranslate tool in either Type & Paste mode (-Type), Screen OCR mode (-Ocr),
    or Clipboard Translation mode (-Clipboard).
.PARAMETER Type
    Opens the interactive live translation popup that automatically translates Arabic to English
    and pastes directly to the previous active window upon pressing Enter (Win+Shift+X).
.PARAMETER Clipboard
    Translates selected / copied text directly (Win+Shift+C).
.PARAMETER Ocr
    Opens screen region snipping selector to OCR & translate text (Win+Shift+Q).
#>
param(
    [switch]$Type,
    [switch]$Clipboard,
    [switch]$Ocr
)

$exePath = Join-Path $PSScriptRoot "quick-translate\QuickTranslate.exe"
if (-not (Test-Path $exePath)) {
    Write-Error "QuickTranslate.exe not found at $exePath. Please run: cd tools\quick-translate ; dotnet publish -c Release -o ..\..\scripts\quick-translate"
    exit 1
}

$argList = @()
if ($Type) {
    $argList += "--type"
} elseif ($Clipboard) {
    $argList += "--clipboard"
}

Start-Process -FilePath $exePath -ArgumentList $argList
