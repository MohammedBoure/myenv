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

    # Check for explicit re-sync / login flags
    $isLoginRequest = $false
    if ($null -ne $ArgsList) {
        foreach ($arg in $ArgsList) {
            if ($arg -in @('--login', '-login', '/login', '--sync', '-sync')) {
                $isLoginRequest = $true
                break
            }
        }
    }

    # Inspect token file
    $token = $null
    if (Test-Path -LiteralPath $tokenFile) {
        $raw = Get-Content -LiteralPath $tokenFile -Raw -ErrorAction SilentlyContinue
        if ($null -ne $raw) {
            $token = $raw.Trim()
        }
    }

    $mutexName = "Global\AgyCredentialMutex"

    # =========================================================================
    # CASE A: Explicit setup or re-sync (agy-sync or missing token)
    # =========================================================================
    if ($isLoginRequest -or [string]::IsNullOrWhiteSpace($token)) {
        if ($isLoginRequest) {
            Write-Host "[$accountName] Re-authentication requested." -ForegroundColor Yellow
        } else {
            Write-Host "[$accountName] Account not configured yet." -ForegroundColor Yellow
        }
        Write-Host "[$accountName] Launching Antigravity CLI. Please complete Google OAuth in your browser..." -ForegroundColor Cyan

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

        # Capture newly created credential ONLY on initial setup / explicit sync
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
    # CASE B: Normal execution with saved token (credential restoration only)
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

# Helper command to re-authenticate or sync any account at will
function global:agy-sync {
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [int]$AccountIndex
    )
    if ($AccountIndex -lt 1 -or $AccountIndex -gt 20) {
        Write-Error "Please specify an account index between 1 and 20 (e.g. agy-sync 1)."
        return 1
    }
    Switch-AgyCredential -AccountIndex $AccountIndex -ArgsList @('--login') | Out-Null
}

# Dynamically define agy1 through agy20 in global scope sharing central settings and sessions
1..20 | ForEach-Object {
    $idx = $_
    $functionBody = @"
function global:agy$idx {
    `$RemainingArgs = `$args

    # 1. Credential switch logic (Mutex + Credential Store restore)
    `$status = Switch-AgyCredential -AccountIndex $idx -ArgsList `$RemainingArgs
    if (`$status -ne 0) {
        if (`$status -eq 100) {
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

    # 3. Synchronize refreshed session token if updated by OAuth
    `$tokenFile = Join-Path `$HOME ".gemini\tokens\agy$idx.token"
    if (Test-Path -LiteralPath `$tokenFile) {
        `$savedToken = Get-Content -LiteralPath `$tokenFile -Raw -ErrorAction SilentlyContinue
        if (`$savedToken) {
            `$expectedEmail = Get-AgyTokenEmail `$savedToken
            `$currentCred = [AgyCredVault]::Read("gemini:antigravity")
            if (`$currentCred -and `$expectedEmail) {
                `$currentEmail = Get-AgyTokenEmail `$currentCred
                if (`$currentEmail -eq `$expectedEmail -and `$currentCred.Trim() -ne `$savedToken.Trim()) {
                    [System.IO.File]::WriteAllText(`$tokenFile, `$currentCred.Trim(), [System.Text.Encoding]::UTF8)
                }
            }
        }
    }
}
"@
    Invoke-Expression $functionBody
}
