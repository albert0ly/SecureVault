# ?? Quick Reference: setup-gcp-credentials.ps1

## What It Does

Automated PowerShell script that sets up Google Cloud credentials for SecureVault.

---

## Usage

```powershell
# Navigate to repository
cd C:\Users\YourName\source\repos\SecureVault

# Run the script
.\setup-gcp-credentials.ps1
```

---

## Features

| Feature | Description |
|---------|-------------|
| **Directory Creation** | Creates `%USERPROFILE%\.gcp` if it doesn't exist |
| **Instructions** | Shows step-by-step GCP Console instructions |
| **Auto-Search** | Finds JSON key files in common locations |
| **Path Validation** | Checks if the key file exists before proceeding |
| **Environment Variable** | Sets `GOOGLE_APPLICATION_CREDENTIALS` permanently |
| **Verification** | Displays current value for confirmation |
| **Error Handling** | Validates input and provides helpful error messages |
| **Color Output** | Easy-to-read colored terminal output |

---

## What You Need Before Running

1. **GCP Service Account Key** downloaded from GCP Console
   - Go to: https://console.cloud.google.com
   - Navigate to: IAM & Admin ? Service Accounts
   - Create key ? JSON format

2. **Key file location** (one of):
   - `C:\Users\YourName\Downloads\` (if just downloaded)
   - `C:\Users\YourName\.gcp\` (recommended secure location)
   - Any custom path on your computer

---

## Step-by-Step Walkthrough

### 1. Script Starts
```
=== SecureVault GCP Credentials Setup ===

? Credentials directory exists: C:\Users\YourName\.gcp
```

### 2. Shows Instructions
```
=== Instructions to Download GCP Service Account Key ===

1. Go to: https://console.cloud.google.com
2. Select your SecureVault project
3. Navigate to: IAM & Admin ? Service Accounts
...
```

### 3. Searches for Keys
```
=== Searching for JSON key files ===
Found JSON files in C:\Users\YourName\Downloads
  - C:\Users\YourName\Downloads\securevault-sa-key.json
```

### 4. Prompts for Path
```
=== Set Environment Variable ===
Please enter the FULL path to your GCP service account key file:
Example: C:\Users\YourName\.gcp\securevault-key.json

Key file path: _
```

**Enter the full path to your key file**

### 5. Validates and Sets
```
? File found: C:\Users\YourName\.gcp\securevault-key.json

Setting GOOGLE_APPLICATION_CREDENTIALS environment variable...
? Environment variable set successfully!

Current value: C:\Users\YourName\.gcp\securevault-key.json
```

### 6. Completion Message
```
=== IMPORTANT ===
Please RESTART Visual Studio for the changes to take effect!

After restarting, verify the setup by running:
  $env:GOOGLE_APPLICATION_CREDENTIALS

? Setup complete!
```

---

## After Running the Script

### 1. Restart Visual Studio
**Important:** Environment variables are only loaded when applications start.

```powershell
# Close Visual Studio completely
# Reopen Visual Studio
```

### 2. Verify Setup

```powershell
# Check environment variable
$env:GOOGLE_APPLICATION_CREDENTIALS

# Should output: C:\Users\YourName\.gcp\securevault-key.json
```

### 3. Run Verification Script

```powershell
.\verify-setup.ps1
```

Expected output:
```
? ALL CHECKS PASSED!
```

### 4. Test Your Application

```powershell
# Run WPF UI
dotnet run --project SecureVaultUI

# Or run Web API
dotnet run --project SecureVaultApi
```

---

## Common Scenarios

### Scenario 1: Key File in Downloads

```powershell
# Key downloaded to: C:\Users\YourName\Downloads\securevault-xxxxx.json

# Script will find it automatically
# Enter this path when prompted:
C:\Users\YourName\Downloads\securevault-xxxxx.json
```

### Scenario 2: Key Already Moved to .gcp

```powershell
# Key at: C:\Users\YourName\.gcp\securevault-key.json

# Script will find it automatically
# Enter this path when prompted:
C:\Users\YourName\.gcp\securevault-key.json
```

### Scenario 3: Key in Custom Location

```powershell
# Key at: D:\MyKeys\gcp\securevault-key.json

# Enter this path when prompted:
D:\MyKeys\gcp\securevault-key.json
```

### Scenario 4: No Key Files Found

```
No JSON key files found. Please download from GCP Console first.

After downloading, run this script again.
```

**Action:** Download the key from GCP Console first, then run the script again.

---

## Troubleshooting

### Issue: Script Exits Immediately

**Cause:** No JSON key files found

**Solution:**
1. Download service account key from GCP Console
2. Save to Downloads or `.gcp` folder
3. Run script again

---

### Issue: "File not found" Error

**Cause:** Path entered incorrectly

**Solution:**
1. Check the path carefully
2. Use Copy-Paste for accuracy
3. Ensure file actually exists: `Test-Path "your\path\here"`

---

### Issue: Environment Variable Not Set After Running

**Cause:** PowerShell session not restarted

**Solution:**
1. Close and reopen PowerShell
2. Or run: `$env:GOOGLE_APPLICATION_CREDENTIALS = [System.Environment]::GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "User")`

---

### Issue: Visual Studio Doesn't See Variable

**Cause:** Visual Studio not restarted after setting variable

**Solution:**
1. **Close Visual Studio completely**
2. **Reopen Visual Studio**
3. Environment variables are loaded at startup

---

## Script Location

```
C:\Users\YourName\source\repos\SecureVault\setup-gcp-credentials.ps1
```

---

## Related Scripts

| Script | Purpose |
|--------|---------|
| `setup-gcp-credentials.ps1` | Set up GCP credentials |
| `verify-setup.ps1` | Verify setup is correct |

---

## What Gets Set

### Environment Variable

```
Name:  GOOGLE_APPLICATION_CREDENTIALS
Value: C:\Users\YourName\.gcp\securevault-key.json
Scope: User (permanent for your user account)
```

### Registry Location

```
HKEY_CURRENT_USER\Environment
GOOGLE_APPLICATION_CREDENTIALS = C:\Users\YourName\.gcp\securevault-key.json
```

---

## Security Notes

? **Good practices:**
- Store key in `%USERPROFILE%\.gcp` (secure user directory)
- Don't commit key file to Git (already in `.gitignore`)
- Don't share key file publicly
- Rotate keys periodically

? **Avoid:**
- Storing key in project directory (if tracked by Git)
- Emailing key files
- Hardcoding paths in code
- Using same key for all environments

---

## Manual Alternative

If you prefer not to use the script:

```powershell
# Set environment variable manually
setx GOOGLE_APPLICATION_CREDENTIALS "C:\Users\YourName\.gcp\securevault-key.json"

# Restart PowerShell/Visual Studio
```

---

## Additional Help

| Need Help With | See |
|----------------|-----|
| Full setup guide | `GCP_SETUP.md` |
| Quick start | `GETTING_STARTED.md` |
| Clone to new computer | `CLONE_SETUP_GUIDE.md` |
| Understanding authentication | `AUTHENTICATION_EXPLAINED.md` |
| Server deployment | `SERVER_DEPLOYMENT_GUIDE.md` |
| General reference | `QUICK_REFERENCE.md` |

---

## Command Summary

```powershell
# Run setup script
.\setup-gcp-credentials.ps1

# Verify setup
.\verify-setup.ps1

# Check environment variable
$env:GOOGLE_APPLICATION_CREDENTIALS

# Test path exists
Test-Path $env:GOOGLE_APPLICATION_CREDENTIALS

# Run application
dotnet run --project SecureVaultUI
```

---

**Last Updated:** Just now  
**Script Version:** 1.0  
**Compatibility:** Windows PowerShell 5.1+, PowerShell 7+
