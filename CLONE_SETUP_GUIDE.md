# ?? SecureVault - Clone Setup Guide

This guide walks you through setting up SecureVault on a new computer after cloning from GitHub.

## ?? Prerequisites

- [ ] Visual Studio 2022 or later
- [ ] .NET 8 SDK installed
- [ ] Git installed
- [ ] Access to GCP Console for your SecureVault project

---

## ?? Quick Setup (5 Steps)

### Step 1: Clone the Repository

```powershell
cd C:\Users\albertly\source\repos
git clone https://github.com/albert0ly/SecureVault
cd SecureVault
```

### Step 2: Download GCP Service Account Key

**Option A: Copy from Existing Computer**

If you have the key file on another computer:

1. On the original computer, find the key location:
   ```powershell
   $env:GOOGLE_APPLICATION_CREDENTIALS
   ```
   Example output: `C:\Users\albertly\.gcp\securevault-key.json`

2. Copy that JSON file to a USB drive or secure cloud storage

3. On the new computer, copy it to:
   ```
   C:\Users\albertly\.gcp\securevault-key.json
   ```

**Option B: Download New Key from GCP Console**

1. ? Go to https://console.cloud.google.com
2. ? Select your **SecureVault project** (e.g., `securevault-12345`)
3. ? Navigate to: **IAM & Admin** ? **Service Accounts**
4. ? Find the service account (e.g., `securevault-sa@...`)
5. ? Click the three dots **?** ? **Manage keys**
6. ? Click **Add Key** ? **Create new key**
7. ? Select **JSON** format
8. ? Click **Create**
9. ? The key file downloads automatically (e.g., `securevault-xxxxx.json`)

### Step 3: Move the Key File to Secure Location

```powershell
# Create credentials directory
mkdir C:\Users\albertly\.gcp -Force

# Move the downloaded key (adjust the filename as needed)
Move-Item "$env:USERPROFILE\Downloads\securevault-*.json" "C:\Users\albertly\.gcp\securevault-key.json"

# Verify the file exists
Test-Path C:\Users\albertly\.gcp\securevault-key.json
```

Should return: `True`

### Step 4: Set Environment Variable

**Run the automated setup script:**

```powershell
cd C:\Users\albertly\source\repos\SecureVault
.\setup-gcp-credentials.ps1
```

**Or set it manually:**

```powershell
# Set permanently (recommended)
setx GOOGLE_APPLICATION_CREDENTIALS "C:\Users\albertly\.gcp\securevault-key.json"
```

You'll see:
```
SUCCESS: Specified value was saved.
```

### Step 5: Restart and Verify

1. **Close and reopen PowerShell** (or restart Visual Studio)

2. **Verify the environment variable:**
   ```powershell
   $env:GOOGLE_APPLICATION_CREDENTIALS
   ```
   
   Should output:
   ```
   C:\Users\albertly\.gcp\securevault-key.json
   ```

3. **Verify the file exists:**
   ```powershell
   Test-Path $env:GOOGLE_APPLICATION_CREDENTIALS
   ```
   
   Should return: `True`

---

## ??? Build and Run

### Build the Solution

```powershell
cd C:\Users\albertly\source\repos\SecureVault
dotnet build SecureVault.sln
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Run the WPF Application

**Option 1: Visual Studio**
1. Open `SecureVault.sln` in Visual Studio
2. Right-click **SecureVaultUI** ? **Set as Startup Project**
3. Press **F5** to run

**Option 2: Command Line**
```powershell
dotnet run --project SecureVaultUI
```

### Run the Web API

```powershell
dotnet run --project SecureVaultApi
```

The API will be available at: `https://localhost:5001`

---

## ? Test the Connection

### Test in WPF Application

1. Enter your **GCP Project ID** (e.g., `securevault-12345`)
2. Click **Connect**
3. Status should show **? Connected** in green
4. Store a test secret:
   - Secret Name: `test-secret`
   - Value: `HelloFromNewComputer`
   - Click **Store Secret**
5. Retrieve it:
   - Secret Name: `test-secret`
   - Click **Retrieve Secret**
   - Should display: `HelloFromNewComputer`

### Test with API

```powershell
# Test health endpoint
curl https://localhost:5001/api/secrets/health

# Store a secret
curl -X POST https://localhost:5001/api/secrets/store `
  -H "Content-Type: application/json" `
  -d '{"secretName":"api-test","secretValue":"TestValue123","projectId":"YOUR_PROJECT_ID"}'

# Retrieve the secret
curl https://localhost:5001/api/secrets/retrieve/api-test?projectId=YOUR_PROJECT_ID
```

---

## ?? Troubleshooting

### Issue: Environment variable returns nothing

```powershell
$env:GOOGLE_APPLICATION_CREDENTIALS
# Returns: (nothing)
```

**Solution:**
- You haven't set the environment variable yet
- Run the setup script: `.\setup-gcp-credentials.ps1`
- Or use: `setx GOOGLE_APPLICATION_CREDENTIALS "C:\Users\albertly\.gcp\securevault-key.json"`
- **Important:** Restart PowerShell/Visual Studio after setting

---

### Issue: "Failed to connect to GCP Secret Manager"

**Solution:**
1. Verify the environment variable is set:
   ```powershell
   $env:GOOGLE_APPLICATION_CREDENTIALS
   ```

2. Check the file exists:
   ```powershell
   Test-Path $env:GOOGLE_APPLICATION_CREDENTIALS
   ```

3. Verify the JSON file is valid:
   ```powershell
   Get-Content $env:GOOGLE_APPLICATION_CREDENTIALS | ConvertFrom-Json | Select-Object type, project_id, client_email
   ```
   
   Should show:
   ```
   type           : service_account
   project_id     : your-project-id
   client_email   : your-sa@your-project.iam.gserviceaccount.com
   ```

4. Ensure Secret Manager API is enabled in GCP Console:
   - Go to https://console.cloud.google.com
   - Navigate to **APIs & Services** ? **Library**
   - Search for "Secret Manager API"
   - Click **Enable** if not already enabled

5. Check service account permissions:
   - Go to **IAM & Admin** ? **IAM**
   - Find your service account
   - Should have role: **Secret Manager Admin**

---

### Issue: "Permission denied" when storing secrets

**Solution:**
- The service account needs **Secret Manager Admin** role
- In GCP Console: **IAM & Admin** ? **IAM**
- Click **Edit** (pencil icon) next to your service account
- Add role: **Secret Manager Admin**
- Click **Save**

---

### Issue: Port 443 connection timeout

**Solution:**
- Your firewall or corporate proxy might be blocking port 443
- The code already includes proxy bypass:
  ```csharp
  UseProxy = false
  ```
- Check with your network admin if port 443 is blocked
- Try using a different network (e.g., mobile hotspot for testing)

---

## ?? File Locations Reference

| Item | Location |
|------|----------|
| **Repository** | `C:\Users\albertly\source\repos\SecureVault` |
| **GCP Key File** | `C:\Users\albertly\.gcp\securevault-key.json` |
| **Setup Script** | `C:\Users\albertly\source\repos\SecureVault\setup-gcp-credentials.ps1` |
| **Solution File** | `C:\Users\albertly\source\repos\SecureVault\SecureVault.sln` |
| **Environment Variable** | `GOOGLE_APPLICATION_CREDENTIALS` |

---

## ?? Security Checklist

- [ ] Key file stored in secure location (`C:\Users\albertly\.gcp\`)
- [ ] Key file NOT in the repository (check `.gitignore`)
- [ ] Environment variable points to correct path
- [ ] File permissions restrict access to your user account only
- [ ] Never commit the JSON key file to Git
- [ ] Never share the key file via email or public channels
- [ ] Rotate keys periodically (every 90 days recommended)

---

## ?? Quick Reference Commands

```powershell
# Check if environment variable is set
$env:GOOGLE_APPLICATION_CREDENTIALS

# Verify file exists
Test-Path $env:GOOGLE_APPLICATION_CREDENTIALS

# View file content (be careful - contains secrets!)
Get-Content $env:GOOGLE_APPLICATION_CREDENTIALS

# Re-run setup script
.\setup-gcp-credentials.ps1

# Build solution
dotnet build

# Run WPF app
dotnet run --project SecureVaultUI

# Run API
dotnet run --project SecureVaultApi

# Search for JSON key files
Get-ChildItem -Path $env:USERPROFILE -Recurse -Filter "*.json" | Where-Object { $_.Name -like "*service*" -or $_.Name -like "*gcp*" }
```

---

## ?? What You Need from Original Computer

If you're moving from another computer, you need:

1. **GCP Service Account Key File** (`.json`)
   - Location: Check `$env:GOOGLE_APPLICATION_CREDENTIALS` on original computer
   - Or download new key from GCP Console

2. **GCP Project ID**
   - Found in GCP Console
   - Or check `GCP_SETUP.md` in the repository

That's it! The source code is already in the GitHub repository.

---

## ? Success Checklist

After completing setup, you should be able to:

- [ ] Environment variable `$env:GOOGLE_APPLICATION_CREDENTIALS` returns a path
- [ ] `Test-Path $env:GOOGLE_APPLICATION_CREDENTIALS` returns `True`
- [ ] Solution builds without errors: `dotnet build`
- [ ] WPF app runs and connects to GCP
- [ ] Can store and retrieve secrets successfully
- [ ] API runs and can handle secret operations

---

## ?? Additional Resources

- **Main Documentation**: `README.md`
- **GCP Setup Details**: `GCP_SETUP.md`
- **Getting Started**: `GETTING_STARTED.md`
- **GCP Console**: https://console.cloud.google.com
- **Secret Manager Docs**: https://cloud.google.com/secret-manager/docs

---

## ?? Still Having Issues?

1. Double-check the environment variable:
   ```powershell
   [System.Environment]::GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "User")
   ```

2. Try setting it manually in System Properties:
   - Press **Win + X** ? **System**
   - **Advanced system settings**
   - **Environment Variables**
   - Under "User variables", add:
     - Variable: `GOOGLE_APPLICATION_CREDENTIALS`
     - Value: `C:\Users\albertly\.gcp\securevault-key.json`

3. Restart your computer (sometimes needed for environment variables to propagate)

4. Check the GCP Console to ensure:
   - Secret Manager API is enabled
   - Service account exists
   - Service account has correct permissions

---

**Ready!** ?? You're all set to use SecureVault on your new computer.
