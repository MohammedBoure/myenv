$utf8 = New-Object System.Text.UTF8Encoding($false)
chcp.com 65001 > $null
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

$env:LANG = "en_US.UTF-8"
$env:LC_ALL = "en_US.UTF-8"
$env:VISUAL = "code --wait"
$env:EDITOR = "code --wait"

if (-not [Console]::IsInputRedirected -and -not [Console]::IsOutputRedirected) {
    try {
        Set-PSReadLineOption -PredictionSource History -ErrorAction Stop
        Set-PSReadLineKeyHandler -Key Ctrl+Backspace -Function BackwardKillWord
    } catch {
        # Some terminal hosts do not expose the PSReadLine capabilities.
    }
}
