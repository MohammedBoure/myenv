<#
.SYNOPSIS
    Direct Full Screen Capture for MyEnv (Alt+Shift+S)
.DESCRIPTION
    Captures entire desktop (multi-monitor aware), saves to Pictures\Screenshots,
    copies the image to Clipboard, and plays a notification sound.
#>

Add-Type -AssemblyName System.Drawing, System.Windows.Forms

try {
    # 1. Capture Virtual Screen (covers multi-monitors seamlessly)
    $screenLeft = [System.Windows.Forms.SystemInformation]::VirtualScreen.Left
    $screenTop = [System.Windows.Forms.SystemInformation]::VirtualScreen.Top
    $screenWidth = [System.Windows.Forms.SystemInformation]::VirtualScreen.Width
    $screenHeight = [System.Windows.Forms.SystemInformation]::VirtualScreen.Height

    $bitmap = New-Object System.Drawing.Bitmap $screenWidth, $screenHeight
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen($screenLeft, $screenTop, 0, 0, $bitmap.Size)

    # 2. Copy image directly to Clipboard
    [System.Windows.Forms.Clipboard]::SetImage($bitmap)

    # 3. Save file to Pictures\Screenshots
    $screenshotsFolder = Join-Path $env:USERPROFILE "Pictures\Screenshots"
    if (-not (Test-Path $screenshotsFolder)) {
        New-Item -ItemType Directory -Path $screenshotsFolder -Force | Out-Null
    }

    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $filePath = Join-Path $screenshotsFolder "Screenshot_$timestamp.png"
    $bitmap.Save($filePath, [System.Drawing.Imaging.ImageFormat]::Png)

    $graphics.Dispose()
    $bitmap.Dispose()

    try {
        [System.Media.SystemSounds]::Asterisk.Play()
    } catch {}
} catch {
    # Non-critical silent catch for background execution
}
