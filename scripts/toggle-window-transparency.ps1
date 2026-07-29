<#
.SYNOPSIS
    Toggle Window Transparency between 80% and 100% for MyEnv (Alt+Shift+O)
#>

$tempFile = "$env:TEMP\glazewm_transparency_state.txt"
$state = ""
if (Test-Path $tempFile) {
    $state = (Get-Content $tempFile -Raw).Trim()
}

if ($state -eq "transparent") {
    glazewm.exe command set-transparency --opacity 100%
    "opaque" | Out-File -FilePath $tempFile -Encoding utf8 -Force
} else {
    glazewm.exe command set-transparency --opacity 80%
    "transparent" | Out-File -FilePath $tempFile -Encoding utf8 -Force
}
