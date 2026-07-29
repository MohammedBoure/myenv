<#
.SYNOPSIS
    Enable Windows 10 Native Active Window Border Color (White Focus Accent)
#>

$dwmPath = "HKCU:\SOFTWARE\Microsoft\Windows\DWM"

# 1. Enable ColorPrevalence (Title bars and window borders accent color)
Set-ItemProperty -Path $dwmPath -Name "ColorPrevalence" -Value 1 -Type DWord

# 2. Set Active Window Border Accent Color to White (0xffffffff in ABGR)
Set-ItemProperty -Path $dwmPath -Name "AccentColor" -Value 0xffffffff -Type DWord

# 3. Set Inactive Window Border Color to Dark Gray (0xff202020 in ABGR)
Set-ItemProperty -Path $dwmPath -Name "AccentColorInactive" -Value 0xff202020 -Type DWord

Write-Host "Windows 10 Native Active Window Border configured to White (#ffffff)." -ForegroundColor Green
