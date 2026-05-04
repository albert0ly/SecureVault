# ??? Server Deployment Guide for SecureVault

This guide covers deploying SecureVault to a **server computer** where interactive login is not available.

---

## Prerequisites

- Windows Server (2019, 2022, or later)
- .NET 8 Runtime installed
- Network access to `secretmanager.googleapis.com` (port 443)
- Administrator privileges

---

## ?? Authentication for Servers

**Servers CANNOT use Application Default Credentials (ADC)** because:
- ? No interactive browser login
- ? No Google Cloud SDK (typically)
- ? Service accounts need to run unattended

**You MUST use Service Account Key with environment variable.**

---

## ?? Step-by-Step Server Setup

### Step 1: Obtain Service Account Key

**On your development computer or GCP Console:**

1. Go to https://console.cloud.google.com
2. Select your project (e.g., `securevault-project`)
3. Navigate to: **IAM & Admin** ? **Service Accounts**
4. Click **Create Service Account**:
   - **Name**: `securevault-server-sa`
   - **Description**: Service account for SecureVault server
   - Click **Create and Continue**
5. Grant role: **Secret Manager Admin**
6. Click **Done**
7. Find the service account in the list
8. Click **?** (three dots) ? **Manage keys**
9. Click **Add Key** ? **Create new key**
10. Select **JSON** format
11. Click **Create**
12. Save the downloaded file (e.g., `securevault-server-sa-key.json`)

---

### Step 2: Transfer Key to Server

**Choose a secure transfer method:**

#### Option A: Secure Copy (SCP)

```powershell
# If you have SSH/SCP access
scp securevault-server-sa-key.json admin@server.example.com:C:\Temp\
```

#### Option B: Network Share

```powershell
# Copy to secure network share
Copy-Item securevault-server-sa-key.json \\server\secure$\keys\
```

#### Option C: Remote Desktop

1. Connect via RDP
2. Copy/paste file through clipboard
3. Or use local drive redirection

#### Option D: Encrypted Storage

1. Encrypt the file with 7-Zip or GPG
2. Upload to secure cloud storage
3. Download on server and decrypt

?? **Never:**
- Email the key file
- Store in public locations
- Commit to Git

---

### Step 3: Install on Server

**On the server (PowerShell as Administrator):**

```powershell
# 1. Create application directory
$appDir = "C:\SecureVault"
New-Item -ItemType Directory -Path $appDir -Force

# 2. Create credentials directory
$credDir = "$appDir\.gcp"
New-Item -ItemType Directory -Path $credDir -Force

# 3. Move key file to secure location
Move-Item "C:\Temp\securevault-server-sa-key.json" "$credDir\securevault-key.json"

# 4. Set restrictive permissions (only SYSTEM and Administrators)
$acl = Get-Acl $credDir
$acl.SetAccessRuleProtection($true, $false)
$acl.Access | ForEach-Object { $acl.RemoveAccessRule($_) }

# Add SYSTEM
$systemRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "NT AUTHORITY\SYSTEM", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
)
$acl.AddAccessRule($systemRule)

# Add Administrators
$adminRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "BUILTIN\Administrators", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
)
$acl.AddAccessRule($adminRule)

Set-Acl $credDir $acl

Write-Host "? Key file secured" -ForegroundColor Green
```

---

### Step 4: Set Environment Variable

**For Web API (runs as user):**

```powershell
# User-level environment variable
[System.Environment]::SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    "$appDir\.gcp\securevault-key.json",
    [System.EnvironmentVariableTarget]::User
)

Write-Host "? Environment variable set (User level)" -ForegroundColor Green
```

**For Windows Service:**

```powershell
# Machine-level environment variable (requires Administrator)
[System.Environment]::SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    "$appDir\.gcp\securevault-key.json",
    [System.EnvironmentVariableTarget]::Machine
)

Write-Host "? Environment variable set (Machine level)" -ForegroundColor Green
Write-Host "? Restart required for services to see this variable" -ForegroundColor Yellow
```

---

### Step 5: Verify Environment Variable

```powershell
# Check user-level
[System.Environment]::GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "User")

# Check machine-level
[System.Environment]::GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Machine")

# Check current process
$env:GOOGLE_APPLICATION_CREDENTIALS
```

---

### Step 6: Deploy Application

#### For Web API (IIS or Kestrel)

```powershell
# Clone repository
cd C:\SecureVault
git clone https://github.com/albert0ly/SecureVault
cd SecureVault

# Build release version
dotnet build -c Release

# Publish for deployment
dotnet publish SecureVaultApi -c Release -o C:\SecureVault\api

# Run directly (for testing)
cd C:\SecureVault\api
dotnet SecureVaultApi.dll

# Should start on https://localhost:5001
```

#### For Windows Service

Install as a Windows Service using `sc.exe` or NSSM:

```powershell
# Download NSSM (Non-Sucking Service Manager)
# https://nssm.cc/download

# Install service
nssm install SecureVaultApi "C:\Program Files\dotnet\dotnet.exe" "C:\SecureVault\api\SecureVaultApi.dll"

# Configure environment
nssm set SecureVaultApi AppEnvironmentExtra "GOOGLE_APPLICATION_CREDENTIALS=C:\SecureVault\.gcp\securevault-key.json"

# Start service
nssm start SecureVaultApi

# Check status
nssm status SecureVaultApi
```

---

### Step 7: Configure Application

Edit `C:\SecureVault\api\appsettings.json`:

```json
{
  "VaultSettings": {
    "Provider": "Gcp",
    "GcpProjectId": "your-actual-project-id"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*",
  "Urls": "https://0.0.0.0:5001"
}
```

---

### Step 8: Test Connection

```powershell
# Test the API endpoint
curl https://localhost:5001/api/secrets/health

# Or from another machine
curl https://server.example.com:5001/api/secrets/health
```

---

## ?? Firewall Configuration

```powershell
# Allow inbound on port 5001 (or your configured port)
New-NetFirewallRule -DisplayName "SecureVault API" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort 5001 `
    -Action Allow

# Allow outbound to GCP (port 443)
New-NetFirewallRule -DisplayName "GCP Secret Manager" `
    -Direction Outbound `
    -Protocol TCP `
    -RemotePort 443 `
    -Action Allow
```

---

## ?? Verify Setup

**Run this verification script on the server:**

```powershell
Write-Host "`n=== SecureVault Server Verification ===" -ForegroundColor Cyan

# 1. Check environment variable
Write-Host "`n[1] Environment Variable:" -ForegroundColor Yellow
$envVar = [System.Environment]::GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Machine")
if ($envVar) {
    Write-Host "  ? Set: $envVar" -ForegroundColor Green
} else {
    Write-Host "  ? NOT SET" -ForegroundColor Red
}

# 2. Check key file
Write-Host "`n[2] Key File:" -ForegroundColor Yellow
if ($envVar -and (Test-Path $envVar)) {
    Write-Host "  ? Exists: $envVar" -ForegroundColor Green
    
    # Validate JSON
    try {
        $key = Get-Content $envVar | ConvertFrom-Json
        Write-Host "  ? Valid JSON" -ForegroundColor Green
        Write-Host "    Project: $($key.project_id)" -ForegroundColor Gray
        Write-Host "    Service Account: $($key.client_email)" -ForegroundColor Gray
    } catch {
        Write-Host "  ? Invalid JSON" -ForegroundColor Red
    }
} else {
    Write-Host "  ? File not found" -ForegroundColor Red
}

# 3. Check .NET Runtime
Write-Host "`n[3] .NET Runtime:" -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "  ? Installed: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "  ? Not installed" -ForegroundColor Red
}

# 4. Check application
Write-Host "`n[4] Application:" -ForegroundColor Yellow
if (Test-Path "C:\SecureVault\api\SecureVaultApi.dll") {
    Write-Host "  ? Deployed" -ForegroundColor Green
} else {
    Write-Host "  ? Not deployed" -ForegroundColor Yellow
}

# 5. Check network connectivity
Write-Host "`n[5] Network Connectivity:" -ForegroundColor Yellow
try {
    $result = Test-NetConnection -ComputerName "secretmanager.googleapis.com" -Port 443 -InformationLevel Quiet
    if ($result) {
        Write-Host "  ? Can reach secretmanager.googleapis.com:443" -ForegroundColor Green
    } else {
        Write-Host "  ? Cannot reach GCP (firewall?)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ? Could not test (no Test-NetConnection)" -ForegroundColor Yellow
}

Write-Host ""
```

---

## ?? Security Best Practices for Servers

### 1. Key File Permissions

```powershell
# Restrict access to key file
icacls "C:\SecureVault\.gcp\securevault-key.json" /inheritance:r
icacls "C:\SecureVault\.gcp\securevault-key.json" /grant "NT AUTHORITY\SYSTEM:(F)"
icacls "C:\SecureVault\.gcp\securevault-key.json" /grant "BUILTIN\Administrators:(F)"
```

### 2. Use Dedicated Service Account

- ? Create separate service account for each server
- ? Grant minimum required permissions (Secret Manager Secret Accessor for read-only)
- ? Rotate keys every 90 days

### 3. Monitor Access

- Enable **Cloud Audit Logs** in GCP Console
- Monitor secret access patterns
- Set up alerts for unusual activity

### 4. Network Security

- Restrict outbound traffic to `*.googleapis.com`
- Use TLS 1.2 or higher
- Consider using Cloud VPN for additional security

---

## ?? Updating the Application

```powershell
# Stop service
nssm stop SecureVaultApi

# Pull latest code
cd C:\SecureVault\SecureVault
git pull

# Rebuild
dotnet publish SecureVaultApi -c Release -o C:\SecureVault\api --force

# Start service
nssm start SecureVaultApi
```

---

## ?? Monitoring

### Application Logs

```powershell
# View logs (if using NSSM)
Get-Content "C:\SecureVault\api\logs\app.log" -Tail 50 -Wait

# Or check Windows Event Viewer
Get-EventLog -LogName Application -Source SecureVaultApi -Newest 20
```

### Health Check Endpoint

```powershell
# Create scheduled task to monitor health
$action = New-ScheduledTaskAction -Execute 'PowerShell.exe' -Argument '-Command "Invoke-WebRequest https://localhost:5001/api/secrets/health"'
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes 5)
Register-ScheduledTask -Action $action -Trigger $trigger -TaskName "SecureVault Health Check"
```

---

## ?? Troubleshooting

### Issue: "Failed to connect to GCP Secret Manager"

**Solution:**
```powershell
# 1. Verify environment variable
$env:GOOGLE_APPLICATION_CREDENTIALS

# 2. Verify key file
Test-Path $env:GOOGLE_APPLICATION_CREDENTIALS

# 3. Test network connectivity
Test-NetConnection secretmanager.googleapis.com -Port 443

# 4. Check service account permissions in GCP Console
```

### Issue: "Permission denied"

**Solution:**
- Verify service account has **Secret Manager Admin** or **Secret Manager Secret Accessor** role
- Check IAM permissions in GCP Console
- Try creating a new key

### Issue: Service won't start

**Solution:**
```powershell
# Check service status
nssm status SecureVaultApi

# View service logs
Get-Content "C:\SecureVault\api\logs\stderr.log"

# Verify .NET Runtime
dotnet --version

# Test manually
cd C:\SecureVault\api
dotnet SecureVaultApi.dll
```

---

## ?? Complete Setup Script

Save this as `server-setup.ps1`:

```powershell
# SecureVault Server Setup Script
# Run as Administrator

param(
    [Parameter(Mandatory=$true)]
    [string]$KeyFilePath,
    
    [Parameter(Mandatory=$true)]
    [string]$GcpProjectId
)

$ErrorActionPreference = "Stop"

Write-Host "=== SecureVault Server Setup ===" -ForegroundColor Cyan

# 1. Create directories
$appDir = "C:\SecureVault"
$credDir = "$appDir\.gcp"
New-Item -ItemType Directory -Path $appDir -Force | Out-Null
New-Item -ItemType Directory -Path $credDir -Force | Out-Null
Write-Host "? Directories created" -ForegroundColor Green

# 2. Copy key file
$targetKeyPath = "$credDir\securevault-key.json"
Copy-Item $KeyFilePath $targetKeyPath -Force
Write-Host "? Key file copied" -ForegroundColor Green

# 3. Set permissions
icacls $credDir /inheritance:r | Out-Null
icacls $credDir /grant "NT AUTHORITY\SYSTEM:(OI)(CI)F" | Out-Null
icacls $credDir /grant "BUILTIN\Administrators:(OI)(CI)F" | Out-Null
Write-Host "? Permissions set" -ForegroundColor Green

# 4. Set environment variable
[System.Environment]::SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    $targetKeyPath,
    [System.EnvironmentVariableTarget]::Machine
)
Write-Host "? Environment variable set" -ForegroundColor Green

# 5. Clone repository
cd $appDir
if (-not (Test-Path "SecureVault")) {
    git clone https://github.com/albert0ly/SecureVault
}
Write-Host "? Repository cloned" -ForegroundColor Green

# 6. Build application
cd SecureVault
dotnet publish SecureVaultApi -c Release -o "$appDir\api"
Write-Host "? Application built" -ForegroundColor Green

# 7. Configure application
$appsettings = Get-Content "$appDir\api\appsettings.json" | ConvertFrom-Json
$appsettings.VaultSettings.Provider = "Gcp"
$appsettings.VaultSettings.GcpProjectId = $GcpProjectId
$appsettings | ConvertTo-Json -Depth 10 | Set-Content "$appDir\api\appsettings.json"
Write-Host "? Application configured" -ForegroundColor Green

Write-Host ""
Write-Host "=== Setup Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Restart the server (for environment variable to take effect)"
Write-Host "2. Test: dotnet $appDir\api\SecureVaultApi.dll"
Write-Host "3. Install as service (optional): See documentation"
Write-Host ""
```

**Usage:**

```powershell
.\server-setup.ps1 -KeyFilePath "C:\Temp\key.json" -GcpProjectId "your-project-id"
```

---

## ? Production Checklist

- [ ] Service account created with minimal permissions
- [ ] Key file transferred securely
- [ ] Key file permissions restricted
- [ ] Environment variable set (Machine-level)
- [ ] Server restarted
- [ ] Application deployed and tested
- [ ] Firewall rules configured
- [ ] Monitoring set up
- [ ] Cloud Audit Logs enabled
- [ ] Key rotation schedule established
- [ ] Backup/recovery plan documented

---

## ?? Additional Resources

- **IIS Deployment**: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/
- **Windows Service**: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service
- **NSSM**: https://nssm.cc/usage
- **GCP Service Accounts**: https://cloud.google.com/iam/docs/service-accounts

---

**Last Updated:** Just now  
**Target:** Windows Server 2019/2022  
**Status:** Production Ready ?
