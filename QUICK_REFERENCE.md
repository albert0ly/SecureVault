# ?? Quick Reference Card: GOOGLE_APPLICATION_CREDENTIALS

## What Is It?

`GOOGLE_APPLICATION_CREDENTIALS` is a **Windows environment variable** that tells Google Cloud libraries where to find your authentication key file.

```
Variable Name:  GOOGLE_APPLICATION_CREDENTIALS
Variable Type:  Environment Variable (User-level)
Purpose:        Point to GCP service account JSON key file
Example Value:  C:\Users\albertly\.gcp\securevault-key.json
```

---

## Quick Check (PowerShell)

```powershell
# Check current session
$env:GOOGLE_APPLICATION_CREDENTIALS

# Check user-level (persistent)
[System.Environment]::GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "User")

# Check if file exists
Test-Path $env:GOOGLE_APPLICATION_CREDENTIALS
```

---

## Quick Setup

### Method 1: Automated Script (Recommended)

```powershell
cd C:\Users\albertly\source\repos\SecureVault
.\setup-gcp-credentials.ps1
```

### Method 2: Manual Command

```powershell
# Set permanently
setx GOOGLE_APPLICATION_CREDENTIALS "C:\Users\albertly\.gcp\securevault-key.json"

# IMPORTANT: Restart PowerShell/Visual Studio after this
```

### Method 3: Windows GUI

1. Press **Win + X** ? **System**
2. Click **Advanced system settings**
3. Click **Environment Variables...**
4. Under "User variables", click **New...**
5. Variable name: `GOOGLE_APPLICATION_CREDENTIALS`
6. Variable value: `C:\Users\albertly\.gcp\securevault-key.json`
7. Click **OK** ? **OK** ? **OK**
8. **Restart Visual Studio**

---

## Troubleshooting Matrix

| Symptom | Check | Fix |
|---------|-------|-----|
| `$env:GOOGLE_APPLICATION_CREDENTIALS` returns nothing | Variable not set | Run `.\setup-gcp-credentials.ps1` |
| Variable set but app fails to connect | File doesn't exist | Download key from GCP Console |
| File exists but app fails | Invalid JSON | Re-download key file |
| Works in PowerShell but not in VS | VS not restarted | **Restart Visual Studio** |
| setx completed but nothing shows | Session not restarted | Close and reopen PowerShell |

---

## The Key File

### What It Looks Like

```json
{
  "type": "service_account",
  "project_id": "your-project-id",
  "private_key_id": "abc123...",
  "private_key": "-----BEGIN PRIVATE KEY-----\n...",
  "client_email": "your-sa@your-project.iam.gserviceaccount.com",
  "client_id": "123456789",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  ...
}
```

### Where to Get It

1. **GCP Console:** https://console.cloud.google.com
2. **IAM & Admin** ? **Service Accounts**
3. Find your service account
4. **?** ? **Manage keys** ? **Add Key** ? **Create new key**
5. Select **JSON** ? **Create**

### Where to Store It

? **Recommended:**
```
C:\Users\albertly\.gcp\securevault-key.json
```

? **Never:**
- In the Git repository
- In the project folder (if tracked by Git)
- In Downloads folder (insecure)
- On desktop or public folders

---

## How It Works in Your Code

Your `GcpSecretManagerVault.cs` uses it like this:

```csharp
// This line automatically looks for GOOGLE_APPLICATION_CREDENTIALS
var credential = await GoogleCredential.GetApplicationDefaultAsync();
```

The Google Cloud SDK automatically:
1. Reads the `GOOGLE_APPLICATION_CREDENTIALS` environment variable
2. Finds the file path from that variable
3. Loads the JSON key file
4. Authenticates your application with GCP

**No additional code needed!** ?

---

## Environment Variable Scope

| Scope | Command | Persistence | Visible To |
|-------|---------|-------------|------------|
| **Process** | `$env:VAR = "value"` | Current window only | Current PowerShell |
| **User** | `setx VAR "value"` | All future sessions | Your user account |
| **Machine** | `setx VAR "value" /M` | All future sessions | All users (needs admin) |

**We use User scope** - perfect balance of security and convenience.

---

## Verification Checklist

```powershell
# Run the automated verification
.\verify-setup.ps1
```

Manual checks:

- [ ] `$env:GOOGLE_APPLICATION_CREDENTIALS` returns a path
- [ ] `Test-Path $env:GOOGLE_APPLICATION_CREDENTIALS` returns `True`
- [ ] File contains valid JSON with `"type": "service_account"`
- [ ] `.NET SDK` is installed (`dotnet --version`)
- [ ] Visual Studio restarted after setting variable

---

## Common Mistakes

### ? Mistake 1: Forgot to Restart
```powershell
setx GOOGLE_APPLICATION_CREDENTIALS "C:\path\to\key.json"
# Immediately runs app without restarting ? FAILS
```

? **Fix:** Restart PowerShell or Visual Studio

---

### ? Mistake 2: Wrong Slash Direction
```powershell
setx GOOGLE_APPLICATION_CREDENTIALS "C:/path/to/key.json"
# Forward slashes might work, but use backslashes on Windows
```

? **Fix:** Use backslashes `C:\path\to\key.json`

---

### ? Mistake 3: Spaces Without Quotes
```powershell
setx GOOGLE_APPLICATION_CREDENTIALS C:\My Documents\key.json
# Fails because of space
```

? **Fix:** Use quotes `"C:\My Documents\key.json"`

---

### ? Mistake 4: File in Project Folder (Git Tracked)
```powershell
setx GOOGLE_APPLICATION_CREDENTIALS "C:\repos\SecureVault\key.json"
# Accidentally commits to Git ? SECURITY ISSUE
```

? **Fix:** Store in user profile `C:\Users\albertly\.gcp\key.json`

---

## One-Liner Status Check

```powershell
if ($env:GOOGLE_APPLICATION_CREDENTIALS) { Write-Host "? SET: $env:GOOGLE_APPLICATION_CREDENTIALS" -ForegroundColor Green; if (Test-Path $env:GOOGLE_APPLICATION_CREDENTIALS) { Write-Host "? FILE EXISTS" -ForegroundColor Green } else { Write-Host "? FILE NOT FOUND" -ForegroundColor Red } } else { Write-Host "? NOT SET" -ForegroundColor Red }
```

---

## Scripts Reference

| Script | Purpose |
|--------|---------|
| `setup-gcp-credentials.ps1` | Interactive setup wizard |
| `verify-setup.ps1` | Check if everything is configured |

---

## Documentation Reference

| File | When to Read |
|------|--------------|
| `SETUP_STATUS.md` | Current status and next steps |
| `CLONE_SETUP_GUIDE.md` | Setting up on a new computer |
| `GCP_SETUP.md` | Detailed GCP configuration |
| `GETTING_STARTED.md` | Quick start guide |

---

## The Bottom Line

**Without `GOOGLE_APPLICATION_CREDENTIALS`:**
```
? Cannot authenticate with GCP
? SecureVaultUI won't connect
? SecureVaultApi won't work with GCP provider
```

**With `GOOGLE_APPLICATION_CREDENTIALS` properly set:**
```
? Automatic GCP authentication
? Store secrets in GCP Secret Manager
? Retrieve secrets from GCP Secret Manager
? Full functionality in UI and API
```

---

**Need to set it up?**  
Run: `.\setup-gcp-credentials.ps1`

**Need to verify?**  
Run: `.\verify-setup.ps1`

**Need help?**  
Read: `CLONE_SETUP_GUIDE.md`

---

*Keep this card handy for quick reference!* ??
