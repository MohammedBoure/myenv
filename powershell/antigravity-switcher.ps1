# ==============================================================================
# Antigravity Multi-Account Switcher (agy1..agy20)
# Centralized MyEnv Module
# ==============================================================================

# Win32 Credential Manager P/Invoke Helper (UTF-8 Compatible)
$agyVaultDefinition = @'
using System;
using System.Runtime.InteropServices;
using System.Text;

public class AgyCredVault {
    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

    [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool CredDelete(string target, int type, int reservedFlag);

    [DllImport("advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
    public static extern void CredFree(IntPtr cred);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CREDENTIAL {
        public int Flags;
        public int Type;
        public string TargetName;
        public string Comment;
        public long LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias;
        public string UserName;
    }

    public static string Read(string target) {
        IntPtr p;
        if (!CredRead(target, 1, 0, out p)) return null;
        try {
            var c = (CREDENTIAL)Marshal.PtrToStructure(p, typeof(CREDENTIAL));
            if (c.CredentialBlobSize <= 0 || c.CredentialBlob == IntPtr.Zero) return string.Empty;
            byte[] b = new byte[c.CredentialBlobSize];
            Marshal.Copy(c.CredentialBlob, b, 0, c.CredentialBlobSize);
            if (b.Length >= 2 && b[1] == 0) {
                return Encoding.Unicode.GetString(b);
            }
            return Encoding.UTF8.GetString(b);
        } finally {
            CredFree(p);
        }
    }

    public static bool Write(string target, string userName, string password) {
        if (password == null) password = string.Empty;
        // Antigravity CLI (Go keyring) requires UTF-8 byte stream
        byte[] blob = Encoding.UTF8.GetBytes(password);
        IntPtr blobPtr = Marshal.AllocHGlobal(blob.Length);
        try {
            Marshal.Copy(blob, 0, blobPtr, blob.Length);
            CREDENTIAL cred = new CREDENTIAL();
            cred.Type = 1; // Generic
            cred.TargetName = target;
            cred.UserName = userName;
            cred.CredentialBlobSize = blob.Length;
            cred.CredentialBlob = blobPtr;
            cred.Persist = 2; // Local machine
            return CredWrite(ref cred, 0);
        } finally {
            Marshal.FreeHGlobal(blobPtr);
        }
    }

    public static bool Delete(string target) {
        return CredDelete(target, 1, 0);
    }
}
'@

if (-not ('AgyCredVault' -as [type])) {
    Add-Type -TypeDefinition $agyVaultDefinition -Language CSharp
}

function Get-AgyTokenEmail {
    param([string]$TokenJson)
    if ([string]::IsNullOrWhiteSpace($TokenJson)) { return $null }
    try {
        $parsed = $TokenJson | ConvertFrom-Json
        if ($parsed.id_token) {
            $parts = $parsed.id_token.Split('.')
            if ($parts.Length -ge 2) {
                $payload = $parts[1]
                while ($payload.Length % 4 -ne 0) { $payload += "=" }
                $payload = $payload.Replace('-', '+').Replace('_', '/')
                $jwtObj = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($payload)) | ConvertFrom-Json
                if ($jwtObj.email) { return $jwtObj.email }
            }
        }
    } catch {}
    return $null
}

function Switch-AgyCredential {
    param(
        [int]$AccountIndex,
        [string[]]$ArgsList
    )

    $accountName = "agy$AccountIndex"
    $tokenDir = Join-Path $HOME ".gemini\tokens"
    $tokenFile = Join-Path $tokenDir "$accountName.token"

    # Check for explicit login or logout requests
    $isLoginRequest = $false
    $isLogoutRequest = $false
    if ($null -ne $ArgsList) {
        foreach ($arg in $ArgsList) {
            if ($arg -in @('login', '--login', '-login', '/login', 'auth', '--auth', '-auth', 'reauth', '--reauth', 'sync', '--sync', '-sync')) {
                $isLoginRequest = $true
                break
            }
            if ($arg -in @('logout', '--logout', '-logout', '/logout', 'clear', '--clear', '-clear', 'reset', '--reset')) {
                $isLogoutRequest = $true
                break
            }
        }
    }

    $mutexName = "Global\AgyCredentialMutex"

    # =========================================================================
    # CASE 1: Explicit Logout / Reset (agy<N> logout or agy-logout <N>)
    # =========================================================================
    if ($isLogoutRequest) {
        if (Test-Path -LiteralPath $tokenFile) {
            Remove-Item -LiteralPath $tokenFile -Force -ErrorAction SilentlyContinue
        }
        $mutex = $null
        $hasLock = $false
        try {
            $mutex = New-Object System.Threading.Mutex($false, $mutexName)
            try { $hasLock = $mutex.WaitOne(5000, $false) } catch [System.Threading.AbandonedMutexException] { $hasLock = $true }
            [AgyCredVault]::Delete("gemini:antigravity") | Out-Null
            Start-Sleep -Milliseconds 150
        } finally {
            if ($hasLock -and $null -ne $mutex) { $mutex.ReleaseMutex() }
            if ($null -ne $mutex) { $mutex.Dispose() }
        }
        Write-Host "[$accountName] Logged out successfully. Slot $AccountIndex cleared." -ForegroundColor Green
        Write-Host "[$accountName] Run '$accountName' or '$accountName login' to sign in with a new Google Account." -ForegroundColor Cyan
        return 101
    }

    # Inspect token file
    $token = $null
    if (Test-Path -LiteralPath $tokenFile) {
        $raw = Get-Content -LiteralPath $tokenFile -Raw -ErrorAction SilentlyContinue
        if ($null -ne $raw) {
            $token = $raw.Trim()
        }
    }

    # =========================================================================
    # CASE 2: Explicit Login or Unconfigured Slot
    # =========================================================================
    if ($isLoginRequest -or [string]::IsNullOrWhiteSpace($token)) {
        if ($isLoginRequest) {
            Write-Host "[$accountName] Re-authentication requested." -ForegroundColor Yellow
        } else {
            Write-Host "[$accountName] Account not configured yet." -ForegroundColor Yellow
        }
        Write-Host "[$accountName] Opening Antigravity CLI to sign in with Google..." -ForegroundColor Cyan
        Write-Host "[$accountName] Hint in browser: Select 'Use another account' if you want to connect a different Gmail." -ForegroundColor Magenta

        $mutex = $null
        $hasLock = $false
        try {
            $mutex = New-Object System.Threading.Mutex($false, $mutexName)
            try { $hasLock = $mutex.WaitOne(5000, $false) } catch [System.Threading.AbandonedMutexException] { $hasLock = $true }
            [AgyCredVault]::Delete("gemini:antigravity") | Out-Null
            Start-Sleep -Milliseconds 150
        } finally {
            if ($hasLock -and $null -ne $mutex) { $mutex.ReleaseMutex() }
            if ($null -ne $mutex) { $mutex.Dispose() }
        }

        & agy.exe

        # Capture newly created credential from Credential Manager
        $capturedToken = [AgyCredVault]::Read("gemini:antigravity")
        if (-not [string]::IsNullOrWhiteSpace($capturedToken)) {
            if (-not (Test-Path -LiteralPath $tokenDir)) {
                New-Item -ItemType Directory -Path $tokenDir -Force | Out-Null
            }
            [System.IO.File]::WriteAllText($tokenFile, $capturedToken.Trim(), [System.Text.Encoding]::UTF8)
            $email = Get-AgyTokenEmail $capturedToken
            if ($email) {
                Write-Host "[$accountName] Session captured successfully for: $email" -ForegroundColor Green
            } else {
                Write-Host "[$accountName] Active session token captured and saved successfully." -ForegroundColor Green
            }
        } else {
            Write-Warning "[$accountName] No session token was detected in Credential Manager after CLI exit."
        }
        return 100
    }

    # =========================================================================
    # CASE 3: Normal Execution with Saved Token (Credential Restoration)
    # =========================================================================
    $mutex = $null
    $hasLock = $false

    try {
        $mutex = New-Object System.Threading.Mutex($false, $mutexName)
        try {
            $hasLock = $mutex.WaitOne(5000, $false)
        } catch [System.Threading.AbandonedMutexException] {
            $hasLock = $true
        }

        if (-not $hasLock) {
            Write-Error "[$accountName] Timeout: Unable to acquire credential lock ($mutexName) within 5000 ms."
            return 1
        }

        # Restore UTF-8 token into Credential Manager via Win32 CredWrite
        $writeSuccess = [AgyCredVault]::Write("gemini:antigravity", "antigravity", $token)
        if (-not $writeSuccess) {
            Write-Error "[$accountName] Failed to write credential to Windows Credential Manager."
            return 1
        }
        Start-Sleep -Milliseconds 150
    }
    finally {
        if ($hasLock -and $null -ne $mutex) {
            $mutex.ReleaseMutex()
        }
        if ($null -ne $mutex) {
            $mutex.Dispose()
        }
    }

    $email = Get-AgyTokenEmail $token
    if ($email) {
        Write-Host "[$accountName] Switched to account: $email" -ForegroundColor Cyan
    }

    return 0
}

# ==============================================================================
# Global Account Management Helper Commands
# ==============================================================================

# Re-authenticate slot with a new Google Account
function global:agy-login {
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [int]$AccountIndex
    )
    if ($AccountIndex -lt 1 -or $AccountIndex -gt 20) {
        Write-Error "Please specify an account index between 1 and 20 (e.g. agy-login 2)."
        return 1
    }
    Switch-AgyCredential -AccountIndex $AccountIndex -ArgsList @('login') | Out-Null
}

# Log out / remove account from slot
function global:agy-logout {
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [int]$AccountIndex
    )
    if ($AccountIndex -lt 1 -or $AccountIndex -gt 20) {
        Write-Error "Please specify an account index between 1 and 20 (e.g. agy-logout 2)."
        return 1
    }
    Switch-AgyCredential -AccountIndex $AccountIndex -ArgsList @('logout') | Out-Null
}

function global:agy-sync {
    param([Parameter(Mandatory = $true, Position = 0)][int]$AccountIndex)
    agy-login $AccountIndex
}

# View all slots and configured Google Accounts in a clean table
function global:agy-list {
    $tokenDir = Join-Path $HOME ".gemini\tokens"
    Write-Host "`n=== Antigravity Multi-Account Slots (agy1..agy20) ===" -ForegroundColor Cyan
    $results = @()
    1..20 | ForEach-Object {
        $slotName = "agy$_"
        $tokenFile = Join-Path $tokenDir "$slotName.token"
        $email = $null
        $status = "Not configured"
        if (Test-Path -LiteralPath $tokenFile) {
            $raw = Get-Content -LiteralPath $tokenFile -Raw -ErrorAction SilentlyContinue
            if (-not [string]::IsNullOrWhiteSpace($raw)) {
                $email = Get-AgyTokenEmail $raw.Trim()
                if ($email) { $status = "Active" } else { $status = "Invalid/Malformed" }
            }
        }
        $results += [PSCustomObject]@{
            Slot   = $slotName
            Email  = if ($email) { $email } else { "-" }
            Status = $status
        }
    }
    $results | Format-Table -AutoSize
    Write-Host "Helper Commands:" -ForegroundColor Gray
    Write-Host "  agy<N>            : Run CLI using account <N> (e.g. agy1)" -ForegroundColor Gray
    Write-Host "  agy<N> login      : Sign in with a new Google Account on slot <N> (e.g. agy2 login)" -ForegroundColor Gray
    Write-Host "  agy<N> logout     : Disconnect account from slot <N> (e.g. agy2 logout)" -ForegroundColor Gray
    Write-Host "  agy-list          : View all slots and configured accounts`n" -ForegroundColor Gray
}

Set-Alias -Name agy-accounts -Value agy-list -Option AllScope -ErrorAction SilentlyContinue

# ==============================================================================
# Dynamically define agy1 through agy20 in global scope
# ==============================================================================
1..20 | ForEach-Object {
    $idx = $_
    $functionBody = @"
function global:agy$idx {
    `$RemainingArgs = `$args

    # 1. Credential switch / login / logout logic
    `$status = Switch-AgyCredential -AccountIndex $idx -ArgsList `$RemainingArgs
    if (`$status -ne 0) {
        if (`$status -in @(100, 101)) {
            `$global:LASTEXITCODE = 0
            return
        }
        `$global:LASTEXITCODE = `$status
        return
    }

    # 2. Transparent Execution in Shared Central Environment (100% parity with agy.exe)
    if (`$MyInvocation.ExpectingInput) {
        `$input | & agy.exe @RemainingArgs
    } else {
        if (`$null -ne `$RemainingArgs -and `$RemainingArgs.Count -gt 0) {
            & agy.exe @RemainingArgs
        } else {
            & agy.exe
        }
    }
    `$global:LASTEXITCODE = `$LASTEXITCODE

    # 3. Synchronize session token (including account changes or OAuth token refresh)
    `$tokenFile = Join-Path `$HOME ".gemini\tokens\agy$idx.token"
    `$currentCred = [AgyCredVault]::Read("gemini:antigravity")
    if (-not [string]::IsNullOrWhiteSpace(`$currentCred)) {
        `$currentEmail = Get-AgyTokenEmail `$currentCred
        `$savedToken = if (Test-Path -LiteralPath `$tokenFile) { Get-Content -LiteralPath `$tokenFile -Raw -ErrorAction SilentlyContinue } else { `$null }
        `$savedEmail = if (`$savedToken) { Get-AgyTokenEmail `$savedToken } else { `$null }

        if (`$currentEmail) {
            if (`$currentEmail -ne `$savedEmail) {
                [System.IO.File]::WriteAllText(`$tokenFile, `$currentCred.Trim(), [System.Text.Encoding]::UTF8)
                Write-Host "[agy$idx] Account updated to: `$currentEmail" -ForegroundColor Green
            } elseif (`$currentCred.Trim() -ne `$savedToken.Trim()) {
                [System.IO.File]::WriteAllText(`$tokenFile, `$currentCred.Trim(), [System.Text.Encoding]::UTF8)
            }
        }
    }
}
"@
    Invoke-Expression $functionBody
}
