# myenv - PowerShell entry profile
$utf8 = New-Object System.Text.UTF8Encoding($false)
chcp.com 65001 > $null
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

$env:LANG = "en_US.UTF-8"
$env:LC_ALL = "en_US.UTF-8"
$env:VISUAL = "code --wait"
$env:EDITOR = "code --wait"

$themePath = "C:\Users\moham\Documents\myenv\powershell\midnight-aurora.ps1"
if (Test-Path $themePath) {
    . $themePath
}