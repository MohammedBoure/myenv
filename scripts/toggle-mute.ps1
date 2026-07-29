<#
.SYNOPSIS
    Toggle Master Volume Mute for MyEnv (Alt+Shift+M)
#>

$code = @"
using System;
using System.Runtime.InteropServices;

public class AudioMute {
    [DllImport("user32.dll")]
    public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    public const int WM_APPCOMMAND = 0x319;
    public const int APPCOMMAND_VOLUME_MUTE = 0x80000;
    public const int HWND_BROADCAST = 0xffff;

    public static void Toggle() {
        SendMessageW((IntPtr)HWND_BROADCAST, WM_APPCOMMAND, (IntPtr)HWND_BROADCAST, (IntPtr)APPCOMMAND_VOLUME_MUTE);
    }
}
"@

Add-Type -TypeDefinition $code
[AudioMute]::Toggle()
