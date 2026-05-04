# ?? How Your SecureVault App Authenticates with GCP

## Current Setup on This Computer

### ? How It Works Right Now

Your app is using **Application Default Credentials (ADC)** from Google Cloud SDK.

```
Authentication Method: Application Default Credentials (ADC)
Credentials File: C:\Users\albertly\AppData\Roaming\gcloud\application_default_credentials.json
Google Cloud SDK: Version 565.0.0 (Installed ?)
Environment Variable: GOOGLE_APPLICATION_CREDENTIALS = (NOT SET)
```

### Why `$env:GOOGLE_APPLICATION_CREDENTIALS` Returns Nothing

You're **not using** the environment variable method. Your app uses a different authentication flow.

---

## ?? How Your Code Finds Credentials

Looking at your `GcpSecretManagerVault.cs`:

```csharp
public async Task ConnectAsync()
{
    // This line searches for credentials in multiple locations
    var credential = await GoogleCredential.GetApplicationDefaultAsync();
    // ...
}
```

### Credential Search Order

`GoogleCredential.GetApplicationDefaultAsync()` looks for credentials in this order:

1. **Environment Variable** `GOOGLE_APPLICATION_CREDENTIALS`
   - ? Not set on your computer
   - Would point to a service account JSON key file

2. **Application Default Credentials (ADC)** ? **YOU ARE HERE**
   - ? Found at: `C:\Users\albertly\AppData\Roaming\gcloud\application_default_credentials.json`
   - Created when you ran: `gcloud auth application-default login`
   - Uses your personal Google account

3. **Compute Engine/GKE Metadata Server**
   - ? Not applicable (only works on GCP infrastructure)

**Result:** Your app uses **#2 - Application Default Credentials**

---

## ?? How ADC Was Created on Your Computer

You (or someone) ran this command previously:

```sh
gcloud auth application-default login
```

This command:
1. Opened a browser window
2. Asked you to log in with your Google account
3. Saved the credentials to `%APPDATA%\gcloud\application_default_credentials.json`
4. Now your app automatically finds and uses these credentials

---

## ?? Cloning to Another Computer - Two Options

### Option A: Use Application Default Credentials (Same Method)

**What you need on the new computer:**

1. **Install Google Cloud SDK**
   - Download from: https://cloud.google.com/sdk/docs/install
   - Run the installer
   - Add to PATH

2. **Authenticate with your Google account**
   ```sh
   gcloud auth application-default login
   ```
   
3. **Clone and run your app**
   ```powershell
   git clone https://github.com/albert0ly/SecureVault
   cd SecureVault
   dotnet build
   dotnet run --project SecureVaultUI
   ```

**Pros:**
- ? Quick setup
- ? Uses your personal Google account
- ? Same method as original computer
- ? Good for development

**Cons:**
- ? Requires Google Cloud SDK installation (~400 MB)
- ? Requires interactive browser login
- ? Not suitable for servers/automation
- ? Credentials tied to your personal account

---

### Option B: Use Service Account Key (Better for Production)

**What you need:**

1. **Create a service account key** (one-time setup in GCP Console)
2. **Download the JSON key file** (small, ~2 KB)
3. **Set environment variable** on new computer

**Steps:**

#### On GCP Console (Do Once)

1. Go to https://console.cloud.google.com
2. Select your project
3. Navigate to: **IAM & Admin** ? **Service Accounts**
4. Click **Create Service Account** or use existing one
5. Give it **Secret Manager Admin** role
6. Click **Keys** tab ? **Add Key** ? **Create new key**
7. Select **JSON** format
8. Download the file (e.g., `securevault-sa-key.json`)

#### On New Computer

```powershell
# Clone the repository
git clone https://github.com/albert0ly/SecureVault
cd SecureVault

# Copy the key file to secure location
# (e.g., from USB drive or secure cloud)
Copy-Item "path\to\securevault-sa-key.json" "$env:USERPROFILE\.gcp\securevault-key.json"

# Run the setup script
.\setup-gcp-credentials.ps1

# Or set manually
setx GOOGLE_APPLICATION_CREDENTIALS "$env:USERPROFILE\.gcp\securevault-key.json"

# Restart PowerShell, then build and run
dotnet build
dotnet run --project SecureVaultUI
```

**Pros:**
- ? No Google Cloud SDK needed
- ? Works on any machine
- ? Good for production/servers
- ? Portable key file (2 KB)
- ? Can be automated
- ? Credentials not tied to personal account

**Cons:**
- ? Need to manage key file securely
- ? Need to set environment variable
- ? One-time setup in GCP Console

---

## ?? Recommendation

### For Development/Personal Use
? **Option A** (Application Default Credentials)

### For Production/Multiple Computers/CI/CD
? **Option B** (Service Account Key with environment variable)

---

## ?? Comparison Table

| Feature | Option A (ADC) | Option B (Service Account) |
|---------|----------------|----------------------------|
| **Requires Google Cloud SDK** | ? Yes (~400 MB) | ? No |
| **Requires Browser Login** | ? Yes | ? No |
| **Setup Complexity** | Easy (one command) | Medium (GCP Console + env var) |
| **Portable** | ? No (requires SDK) | ? Yes (small JSON file) |
| **Production Ready** | ? No | ? Yes |
| **Works on Servers** | ? No (interactive login) | ? Yes |
| **File Size** | N/A (SDK install) | ~2 KB (JSON file) |
| **Authentication Type** | Personal Google Account | Service Account |
| **Can be Automated** | ? No | ? Yes |

---

## ?? Verify Your Current Setup

Run these commands to see your current authentication:

```powershell
# Check for ADC file
Test-Path "$env:APPDATA\gcloud\application_default_credentials.json"

# Check for environment variable
$env:GOOGLE_APPLICATION_CREDENTIALS

# Check gcloud installation
gcloud --version

# View ADC file content (be careful - contains credentials!)
Get-Content "$env:APPDATA\gcloud\application_default_credentials.json" | ConvertFrom-Json | Select-Object type, client_id, project_id
```

---

## ?? Quick Setup Commands

### For Option A (Same as Your Current Setup)

**On new computer:**

```sh
# Install Google Cloud SDK first, then:
gcloud auth application-default login
```

### For Option B (Service Account Key)

**On new computer:**

```powershell
# After copying the key file:
.\setup-gcp-credentials.ps1
```

---

## ?? Security Notes

### Application Default Credentials (Option A)
- ? Credentials stored in user profile (secure)
- ? Automatically refreshed by gcloud
- ?? Uses your personal Google account
- ?? Permissions based on your IAM role

### Service Account Key (Option B)
- ? Dedicated service account
- ? Limited permissions (only what you grant)
- ? Can be revoked without affecting your account
- ?? Key file must be kept secure
- ?? Should not be committed to Git (already in `.gitignore`)

---

## ?? Additional Resources

- **Google Cloud SDK Installation**: https://cloud.google.com/sdk/docs/install
- **Application Default Credentials**: https://cloud.google.com/docs/authentication/application-default-credentials
- **Service Accounts**: https://cloud.google.com/iam/docs/service-accounts
- **Best Practices**: https://cloud.google.com/iam/docs/best-practices-service-accounts

---

## ?? Summary

**On your original computer:**
- ? Google Cloud SDK installed (version 565.0.0)
- ? Authenticated via: `gcloud auth application-default login`
- ? Credentials file: `%APPDATA%\gcloud\application_default_credentials.json`
- ? Environment variable `GOOGLE_APPLICATION_CREDENTIALS` not used

**To clone to another computer:**
- **Easy way**: Install Google Cloud SDK + run `gcloud auth application-default login`
- **Better way**: Create service account key + set `GOOGLE_APPLICATION_CREDENTIALS`

**Both methods work with your existing code** - no code changes needed! ?

---

**Last Updated:** Just now  
**Your Computer:** Using Application Default Credentials ?  
**Status:** Working perfectly ?
