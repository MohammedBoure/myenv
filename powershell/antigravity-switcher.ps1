# ==============================================================================
# Antigravity Multi-Account & Multi-Instance Switcher (agys)
# Centralized MyEnv Module - Full Isolation Architecture
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
    $helpersDll = Join-Path $PSScriptRoot "MyEnv.Helpers.dll"
    if (-not (Test-Path -LiteralPath $helpersDll)) {
        $helpersDll = "$env:USERPROFILE\Documents\myenv\powershell\MyEnv.Helpers.dll"
    }
    if (Test-Path -LiteralPath $helpersDll) {
        [System.Reflection.Assembly]::LoadFrom($helpersDll) | Out-Null
    }
}
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

function Initialize-AgyInstance {
    param(
        [Parameter(Mandatory = $true)]
        [int]$Index
    )

    $realUserProfile = [Environment]::GetFolderPath('UserProfile')
    $instancesRoot = Join-Path $realUserProfile ".gemini\instances"
    $instanceDir = Join-Path $instancesRoot "agy$Index"
    $geminiDir = Join-Path $instanceDir ".gemini"
    $cliDir = Join-Path $geminiDir "antigravity-cli"
    $convDir = Join-Path $cliDir "conversations"
    $tokensDir = Join-Path $realUserProfile ".gemini\tokens"
    $legacyToken = Join-Path $tokensDir "agy$Index.token"
    $instanceToken = Join-Path $instanceDir "token.json"

    $isNew = $false
    if (-not (Test-Path -LiteralPath $instanceDir)) {
        $isNew = $true
        Write-Host "[agy$Index] First-time setup: Initializing isolated environment for agy$Index..." -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $convDir -Force | Out-Null

        # Link SSH directory for shared Git/SSH keys
        $realSsh = Join-Path $realUserProfile ".ssh"
        $instanceSsh = Join-Path $instanceDir ".ssh"
        if ((Test-Path -LiteralPath $realSsh) -and (-not (Test-Path -LiteralPath $instanceSsh))) {
            cmd /c "mklink /J `"$instanceSsh`" `"$realSsh`"" 2>$null | Out-Null
        }

        # Link global myenv config (rules, skills, MCP configuration)
        $myenvConfig = Join-Path $realUserProfile "Documents\myenv\gemini"
        $instanceConfig = Join-Path $geminiDir "config"
        if ((Test-Path -LiteralPath $myenvConfig) -and (-not (Test-Path -LiteralPath $instanceConfig))) {
            cmd /c "mklink /J `"$instanceConfig`" `"$myenvConfig`"" 2>$null | Out-Null
        }

        # Seed settings.json from global template if available
        $globalSettings = Join-Path $realUserProfile ".gemini\antigravity-cli\settings.json"
        $instanceSettings = Join-Path $cliDir "settings.json"
        if ((Test-Path -LiteralPath $globalSettings) -and (-not (Test-Path -LiteralPath $instanceSettings))) {
            Copy-Item -LiteralPath $globalSettings -Destination $instanceSettings -Force -ErrorAction SilentlyContinue
        }

        Write-Host "[agy$Index] Created isolated .gemini profile at: $instanceDir" -ForegroundColor Green
    }

    # Automatically migrate legacy token if present
    if ((-not (Test-Path -LiteralPath $instanceToken)) -and (Test-Path -LiteralPath $legacyToken)) {
        Copy-Item -LiteralPath $legacyToken -Destination $instanceToken -Force -ErrorAction SilentlyContinue
    }

    return [PSCustomObject]@{
        InstanceDir   = $instanceDir
        InstanceToken = $instanceToken
        LegacyToken   = $legacyToken
        IsNew         = $isNew
    }
}

function Invoke-AgyInstance {
    param(
        [Parameter(Mandatory = $true)]
        [int]$Index,
        [string[]]$ArgsList
    )

    $slotName = "agy$Index"
    $init = Initialize-AgyInstance -Index $Index
    $instanceDir = $init.InstanceDir
    $tokenFile = $init.InstanceToken
    $legacyTokenFile = $init.LegacyToken

    # Parse explicit login or logout requests
    $isLoginRequest = $false
    $isLogoutRequest = $false
    $filteredArgs = @()

    if ($null -ne $ArgsList) {
        foreach ($arg in $ArgsList) {
            if ($arg -in @('login', '--login', '-login', '/login', 'auth', '--auth', '-auth', 'reauth', '--reauth')) {
                $isLoginRequest = $true
                continue
            }
            if ($arg -in @('logout', '--logout', '-logout', '/logout', 'clear', '--clear', '-clear', 'reset', '--reset')) {
                $isLogoutRequest = $true
                continue
            }
            $filteredArgs += $arg
        }
    }

    $mutexName = "Global\AgyCredentialMutex"

    # =========================================================================
    # CASE 1: Logout / Reset
    # =========================================================================
    if ($isLogoutRequest) {
        if (Test-Path -LiteralPath $tokenFile) {
            Remove-Item -LiteralPath $tokenFile -Force -ErrorAction SilentlyContinue
        }
        if (Test-Path -LiteralPath $legacyTokenFile) {
            Remove-Item -LiteralPath $legacyTokenFile -Force -ErrorAction SilentlyContinue
        }
        $mutex = $null
        $hasLock = $false
        try {
            $mutex = New-Object System.Threading.Mutex($false, $mutexName)
            try { $hasLock = $mutex.WaitOne(5000, $false) } catch [System.Threading.AbandonedMutexException] { $hasLock = $true }
            [AgyCredVault]::Delete("gemini:antigravity") | Out-Null
            Start-Sleep -Milliseconds 100
        } finally {
            if ($hasLock -and $null -ne $mutex) { $mutex.ReleaseMutex() }
            if ($null -ne $mutex) { $mutex.Dispose() }
        }
        Write-Host "[$slotName] Logged out successfully. Slot $Index credentials cleared." -ForegroundColor Green
        Write-Host "[$slotName] Run '$slotName' or '$slotName login' to connect a new Google Account." -ForegroundColor Cyan
        return
    }

    # Inspect token file
    $token = $null
    if (Test-Path -LiteralPath $tokenFile) {
        $raw = Get-Content -LiteralPath $tokenFile -Raw -ErrorAction SilentlyContinue
        if (-not [string]::IsNullOrWhiteSpace($raw)) {
            $token = $raw.Trim()
        }
    }

    # =========================================================================
    # CASE 2: Login or Unconfigured Slot
    # =========================================================================
    if ($isLoginRequest -or [string]::IsNullOrWhiteSpace($token)) {
        if ($isLoginRequest) {
            Write-Host "[$slotName] Re-authentication requested for slot $Index." -ForegroundColor Yellow
        } else {
            Write-Host "[$slotName] Slot $Index is not configured with a Google Account yet." -ForegroundColor Yellow
        }
        Write-Host "[$slotName] Opening Antigravity CLI for Google Authentication..." -ForegroundColor Cyan
        Write-Host "[$slotName] >>> IMPORTANT: In your browser, choose 'Use another account' to connect the distinct Gmail for $slotName! <<<" -ForegroundColor Magenta

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

        # Run agy.exe with isolated USERPROFILE
        $origUserProfile = $env:USERPROFILE
        $origHome = $env:HOME
        try {
            $env:USERPROFILE = $instanceDir
            $env:HOME = $instanceDir
            if ($filteredArgs.Count -gt 0) {
                & agy.exe @filteredArgs
            } else {
                & agy.exe
            }
        } finally {
            $env:USERPROFILE = $origUserProfile
            $env:HOME = $origHome
        }

        # Capture newly created credential from Credential Manager
        $capturedToken = [AgyCredVault]::Read("gemini:antigravity")
        if (-not [string]::IsNullOrWhiteSpace($capturedToken)) {
            $parentTokenDir = Split-Path $legacyTokenFile
            if (-not (Test-Path -LiteralPath $parentTokenDir)) {
                New-Item -ItemType Directory -Path $parentTokenDir -Force | Out-Null
            }
            [System.IO.File]::WriteAllText($tokenFile, $capturedToken.Trim(), [System.Text.Encoding]::UTF8)
            [System.IO.File]::WriteAllText($legacyTokenFile, $capturedToken.Trim(), [System.Text.Encoding]::UTF8)
            $newEmail = Get-AgyTokenEmail $capturedToken

            # Check for duplicate accounts across other slots
            $otherDupes = @()
            $parentInstances = Split-Path $instanceDir
            if (Test-Path -LiteralPath $parentInstances) {
                Get-ChildItem -LiteralPath $parentInstances -Directory -ErrorAction SilentlyContinue | ForEach-Object {
                    if ($_.Name -ne $slotName) {
                        $otherTokenPath = Join-Path $_.FullName "token.json"
                        if (Test-Path -LiteralPath $otherTokenPath) {
                            $ot = Get-Content -LiteralPath $otherTokenPath -Raw -ErrorAction SilentlyContinue
                            $oe = Get-AgyTokenEmail $ot
                            if ($oe -and ($oe -ieq $newEmail)) { $otherDupes += $_.Name }
                        }
                    }
                }
            }

            if ($newEmail) {
                if ($otherDupes.Count -gt 0) {
                    Write-Host "`n[!] WARNING: Account $newEmail is ALREADY linked to: $($otherDupes -join ', ')!" -ForegroundColor Yellow
                    Write-Host "    To make $slotName completely separate, run '$slotName login' and select a different Google Account.`n" -ForegroundColor Yellow
                } else {
                    Write-Host "[$slotName] Linked successfully to unique account: $newEmail" -ForegroundColor Green
                }
            } else {
                Write-Host "[$slotName] Session token captured and stored successfully." -ForegroundColor Green
            }
        } else {
            Write-Warning "[$slotName] No session token was detected in Credential Manager after exit."
        }
        $global:LASTEXITCODE = 0
        return
    }

    # =========================================================================
    # CASE 3: Normal Execution with Saved Token
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
            Write-Error "[$slotName] Timeout: Unable to acquire credential lock ($mutexName) within 5000 ms."
            return 1
        }

        # Restore UTF-8 token into Credential Manager
        $writeSuccess = [AgyCredVault]::Write("gemini:antigravity", "antigravity", $token)
        if (-not $writeSuccess) {
            Write-Error "[$slotName] Failed to write credential to Windows Credential Manager."
            return 1
        }
        Start-Sleep -Milliseconds 100
    } finally {
        if ($hasLock -and $null -ne $mutex) {
            $mutex.ReleaseMutex()
        }
        if ($null -ne $mutex) {
            $mutex.Dispose()
        }
    }

    $email = Get-AgyTokenEmail $token
    if ($email) {
        Write-Host "[$slotName] Account: $email" -ForegroundColor Cyan
    }

    # Execute agy.exe with isolated USERPROFILE
    $origUserProfile = $env:USERPROFILE
    $origHome = $env:HOME

    try {
        $env:USERPROFILE = $instanceDir
        $env:HOME = $instanceDir

        if ($MyInvocation.ExpectingInput) {
            $input | & agy.exe @filteredArgs
        } else {
            if ($filteredArgs.Count -gt 0) {
                & agy.exe @filteredArgs
            } else {
                & agy.exe
            }
        }
        $global:LASTEXITCODE = $LASTEXITCODE
    } finally {
        $env:USERPROFILE = $origUserProfile
        $env:HOME = $origHome
    }

    # Synchronize session token if refreshed during execution
    $currentCred = [AgyCredVault]::Read("gemini:antigravity")
    if (-not [string]::IsNullOrWhiteSpace($currentCred)) {
        $currentEmail = Get-AgyTokenEmail $currentCred
        if ($currentEmail -and $email -and ($currentEmail -ieq $email) -and ($currentCred.Trim() -ne $token.Trim())) {
            [System.IO.File]::WriteAllText($tokenFile, $currentCred.Trim(), [System.Text.Encoding]::UTF8)
            [System.IO.File]::WriteAllText($legacyTokenFile, $currentCred.Trim(), [System.Text.Encoding]::UTF8)
        }
    }
}

# ==============================================================================
# Overview & Management Function: agys
# ==============================================================================

function Get-AgyInstanceList {
    $realUserProfile = [Environment]::GetFolderPath('UserProfile')
    $instancesRoot = Join-Path $realUserProfile ".gemini\instances"
    $tokensDir = Join-Path $realUserProfile ".gemini\tokens"

    $slots = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    if (Test-Path -LiteralPath $instancesRoot) {
        Get-ChildItem -LiteralPath $instancesRoot -Directory -ErrorAction SilentlyContinue | ForEach-Object {
            if ($_.Name -match '^agy(\d+)$') { $slots.Add($_.Name) | Out-Null }
        }
    }
    if (Test-Path -LiteralPath $tokensDir) {
        Get-ChildItem -LiteralPath $tokensDir -Filter "*.token" -ErrorAction SilentlyContinue | ForEach-Object {
            $base = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
            if ($base -match '^agy(\d+)$') { $slots.Add($base) | Out-Null }
        }
    }

    $sortedSlots = $slots | Sort-Object { if ($_ -match '^agy(\d+)$') { [int]$Matches[1] } else { 999999 } }

    $items = @()
    $emailCounts = @{}

    foreach ($slot in $sortedSlots) {
        $instanceDir = Join-Path $instancesRoot $slot
        $instanceToken = Join-Path $instanceDir "token.json"
        $legacyToken = Join-Path $tokensDir "$slot.token"
        $tokenRaw = $null

        if (Test-Path -LiteralPath $instanceToken) {
            $tokenRaw = Get-Content -LiteralPath $instanceToken -Raw -ErrorAction SilentlyContinue
        } elseif (Test-Path -LiteralPath $legacyToken) {
            $tokenRaw = Get-Content -LiteralPath $legacyToken -Raw -ErrorAction SilentlyContinue
        }

        $email = if ($tokenRaw) { Get-AgyTokenEmail $tokenRaw } else { $null }
        if ($email) {
            if (-not $emailCounts.ContainsKey($email)) { $emailCounts[$email] = 0 }
            $emailCounts[$email]++
        }

        $sessionsCount = 0
        $convDir = Join-Path $instanceDir ".gemini\antigravity-cli\conversations"
        if (Test-Path -LiteralPath $convDir) {
            $sessionsCount = (Get-ChildItem -LiteralPath $convDir -Filter "*.db" -ErrorAction SilentlyContinue).Count
        }

        $created = "-"
        if (Test-Path -LiteralPath $instanceDir) {
            $created = (Get-Item -LiteralPath $instanceDir).CreationTime.ToString("yyyy-MM-dd HH:mm")
        }

        $status = "Needs Login"
        if ($email) {
            $status = "Active"
        } elseif ($tokenRaw) {
            $status = "Invalid"
        }

        $items += [PSCustomObject]@{
            Slot     = $slot
            Email    = if ($email) { $email } else { "-" }
            Sessions = $sessionsCount
            Status   = $status
            Created  = $created
            RawEmail = $email
        }
    }

    $hasDuplicates = $false
    $results = @()
    foreach ($it in $items) {
        $dupeFlag = "-"
        if ($it.RawEmail -and $emailCounts[$it.RawEmail] -gt 1) {
            $dupeFlag = "[!] Shared ($($emailCounts[$it.RawEmail]) slots)"
            $hasDuplicates = $true
        }
        $results += [PSCustomObject]@{
            Slot      = $it.Slot
            Email     = $it.Email
            Sessions  = $it.Sessions
            Status    = $it.Status
            Duplicate = $dupeFlag
            Created   = $it.Created
        }
    }

    Write-Host "`n==========================================================================================" -ForegroundColor Cyan
    Write-Host "                    Antigravity Multi-Instance & Account Manager (agys)                    " -ForegroundColor Cyan
    Write-Host "==========================================================================================" -ForegroundColor Cyan

    if ($results.Count -eq 0) {
        Write-Host "No agy instances initialized yet. Type 'agy1' or 'agy2' to create your first instance.`n" -ForegroundColor Yellow
    } else {
        $results | Format-Table -AutoSize
    }

    if ($hasDuplicates) {
        Write-Host "[!] Warning: Multiple slots are using the exact same Google Account." -ForegroundColor Yellow
        Write-Host "    To separate them with distinct accounts, run: agy<N> login (e.g. 'agy2 login') and choose 'Use another account'.`n" -ForegroundColor Gray
    }

    Write-Host "Quick Commands:" -ForegroundColor Gray
    Write-Host "  agy<N>             : Run instance <N> (e.g. agy1, agy2, agy100)" -ForegroundColor Gray
    Write-Host "  agy<N> login       : Sign in with a distinct Google Account for slot <N>" -ForegroundColor Gray
    Write-Host "  agy<N> logout      : Disconnect account from slot <N>" -ForegroundColor Gray
    Write-Host "  agys -l            : List all instances, accounts, and session counts" -ForegroundColor Gray
    Write-Host "  agys rm <N>        : Delete instance folder and credentials for slot <N>`n" -ForegroundColor Gray
}

function global:agys {
    param(
        [Parameter(Position = 0)]
        [string]$Action = "list",
        [Parameter(Position = 1)]
        [string]$Target,
        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$RemainingArgs
    )

    if ($Action -in @('-l', '--list', 'list', 'ls', 'status') -or [string]::IsNullOrWhiteSpace($Action)) {
        Get-AgyInstanceList
        return
    }

    if ($Action -in @('login', 'auth', 'reauth') -and $Target -match '^\d+$') {
        Invoke-AgyInstance -Index ([int]$Target) -ArgsList @('login')
        return
    }

    if ($Action -in @('logout', 'clear', 'reset') -and $Target -match '^\d+$') {
        Invoke-AgyInstance -Index ([int]$Target) -ArgsList @('logout')
        return
    }

    if ($Action -in @('rm', 'remove', 'delete') -and $Target -match '^\d+$') {
        $idx = [int]$Target
        $realUserProfile = [Environment]::GetFolderPath('UserProfile')
        $instDir = Join-Path $realUserProfile ".gemini\instances\agy$idx"
        $tokenFile = Join-Path $realUserProfile ".gemini\tokens\agy$idx.token"

        if (Test-Path -LiteralPath $instDir) {
            Remove-Item -LiteralPath $instDir -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "[agy$idx] Instance directory removed: $instDir" -ForegroundColor Green
        }
        if (Test-Path -LiteralPath $tokenFile) {
            Remove-Item -LiteralPath $tokenFile -Force -ErrorAction SilentlyContinue
            Write-Host "[agy$idx] Token file removed." -ForegroundColor Green
        }
        return
    }

    if ($Action -in @('run', 'exec') -and $Target -match '^\d+$') {
        Invoke-AgyInstance -Index ([int]$Target) -ArgsList $RemainingArgs
        return
    }

    if ($Action -match '^agy?(\d+)$') {
        $idx = [int]$Matches[1]
        $passArgs = @()
        if ($Target) { $passArgs += $Target }
        if ($RemainingArgs) { $passArgs += $RemainingArgs }
        Invoke-AgyInstance -Index $idx -ArgsList $passArgs
        return
    }

    Write-Host "Unknown agys command: $Action" -ForegroundColor Red
    Write-Host "Usage: agys [-l] | agys login <N> | agys logout <N> | agys rm <N> | agys run <N>" -ForegroundColor Gray
}

# Aliases
Set-Alias -Name agy-list -Value agys -Option AllScope -ErrorAction SilentlyContinue
Set-Alias -Name agy-accounts -Value agys -Option AllScope -ErrorAction SilentlyContinue

function global:agy-login {
    param([Parameter(Mandatory = $true, Position = 0)][int]$AccountIndex)
    Invoke-AgyInstance -Index $AccountIndex -ArgsList @('login')
}

function global:agy-logout {
    param([Parameter(Mandatory = $true, Position = 0)][int]$AccountIndex)
    Invoke-AgyInstance -Index $AccountIndex -ArgsList @('logout')
}

function global:agy-sync {
    param([Parameter(Mandatory = $true, Position = 0)][int]$AccountIndex)
    Invoke-AgyInstance -Index $AccountIndex -ArgsList @('login')
}

# ==============================================================================
# Dynamic Registration of agy1 through agy200 + any existing custom slot
# ==============================================================================

function Register-AgySlot {
    param([int]$Index)
    $cmd = "function global:agy$Index { [CmdletBinding()] param([Parameter(ValueFromRemainingArguments = `$true)]`$RemainingArgs) Invoke-AgyInstance -Index $Index -ArgsList `$RemainingArgs }"
    Invoke-Expression $cmd
}

# Pre-register 1..200
1..200 | ForEach-Object { Register-AgySlot -Index $_ }

# Also scan any existing slots in instances or tokens directories beyond 200
$realUserProfile = [Environment]::GetFolderPath('UserProfile')
$existingDirs = Join-Path $realUserProfile ".gemini\instances"
if (Test-Path -LiteralPath $existingDirs) {
    Get-ChildItem -LiteralPath $existingDirs -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        if ($_.Name -match '^agy(\d+)$') {
            $num = [int]$Matches[1]
            if ($num -gt 200) { Register-AgySlot -Index $num }
        }
    }
}
