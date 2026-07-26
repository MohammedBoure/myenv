# Smart Tiling Direction Handler for GlazeWM 3.x
# Automatically sets tiling direction based on focused window dimensions:
# Width >= Height -> Horizontal split (adds new window to the RIGHT)
# Height > Width  -> Vertical split   (adds new window to the BOTTOM)

$ErrorActionPreference = "SilentlyContinue"

# Start listening to GlazeWM events
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = "glazewm.exe"
$psi.Arguments = "sub --events focus_changed window_managed"
$psi.RedirectStandardOutput = $true
$psi.UseShellExecute = $false
$psi.CreateNoWindow = $true

$process = [System.Diagnostics.Process]::Start($psi)
$reader = $process.StandardOutput

while (-not $reader.EndOfStream) {
    $line = $reader.ReadLine()
    if ([string]::IsNullOrWhitespace($line)) { continue }

    try {
        # Query focused window dimensions directly from GlazeWM
        $focusedJson = glazewm.exe query focused 2>$null
        if ($focusedJson) {
            $focusedObj = $focusedJson | ConvertFrom-Json
            $focused = $focusedObj.data.focused
            
            if ($focused -and $focused.type -eq "window") {
                $w = $focused.width
                $h = $focused.height
                
                if ($w -ge $h) {
                    # Width >= Height: add next window to the RIGHT (horizontal split)
                    glazewm.exe command set-tiling-direction horizontal 2>$null
                } else {
                    # Height > Width: add next window to the BOTTOM (vertical split)
                    glazewm.exe command set-tiling-direction vertical 2>$null
                }
            }
        }
    } catch {
        # Silent ignore on transient parsing errors
    }
}
