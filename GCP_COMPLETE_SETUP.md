# ?? GCP SECRET MANAGER SETUP - COMPLETE WALKTHROUGH

## Prerequisites
- Google Account (create at https://accounts.google.com if needed)
- Free GCP Account (https://cloud.google.com/free)
- Your C# project ready to use

---

## ? STEP 1: Create a GCP Project

### 1.1 Go to Google Cloud Console
```
https://console.cloud.google.com
```

### 1.2 Click on Project Selector (top of page, blue bar)
You'll see something like: `[Select a Project ?]`

### 1.3 Click "NEW PROJECT"

### 1.4 Fill in the form:
- **Project Name**: `SecureVault` (or any name you like)
- **Organization**: Leave blank (default)
- Click **CREATE**

### 1.5 Wait for creation (2-3 seconds)
You'll see notifications at bottom-right saying "Creating project..."

### 1.6 Copy Your Project ID
Once created, you'll see it displayed. Look for:
```
Project Name: SecureVault
Project ID: securevault-12345  ? SAVE THIS!
Project Number: 123456789
```

**Important:** Copy the **Project ID** (not the name) - you'll need this in your app.

---

## ? STEP 2: Enable Secret Manager API

### 2.1 Make sure your new project is selected
Top blue bar should show: `SecureVault`

### 2.2 Go to APIs & Services
In left sidebar:
```
APIs & Services ? Enabled APIs & services
```

### 2.3 Click "+ ENABLE APIS AND SERVICES"
Big blue button at top.

### 2.4 Search for "Secret Manager"
In the search box, type: `Secret Manager`

### 2.5 Click "Google Secret Manager API"
From the search results.

### 2.6 Click "ENABLE"
Blue button on the page.

### 2.7 Wait for it to enable (5-10 seconds)
You'll see a success message.

---

## ? STEP 3: Create a Service Account

### 3.1 Go to Credentials
In left sidebar:
```
APIs & Services ? Credentials
```

### 3.2 Click "+ CREATE CREDENTIALS"
Top blue button.

### 3.3 Select "Service Account"
From the dropdown menu.

### 3.4 Fill in Service Account Details

**Page 1 of 3:**
- **Service Account Name**: `secure-vault-app`
- **Service Account ID**: Auto-filled, leave it
- Click **CREATE AND CONTINUE**

**Page 2 of 3 - Grant Permissions:**
- **Select a role**: Click the dropdown
- Search for: `Secret Manager Admin`
- Click to select it
- Click **CONTINUE**

**Page 3 of 3:**
- Leave everything blank/default
- Click **DONE**

You're now back at Credentials page.

---

## ? STEP 4: Create and Download Service Account Key

### 4.1 Find Your Service Account
Look for `secure-vault-app` in the "Service Accounts" section.
Click on the **email address** (looks like: `secure-vault-app@securevault-12345.iam.gserviceaccount.com`)

### 4.2 Go to "KEYS" Tab
At the top of the page, click: **KEYS**

### 4.3 Click "Add Key" ? "Create new key"
- Choose **JSON** (should be default)
- Click **CREATE**

### 4.4 JSON File Downloads
A file will download automatically (e.g., `secure-vault-app-xxxxx.json`)

**IMPORTANT:**
- ? Save this file somewhere safe on your computer
- ? NEVER commit this file to Git
- ? NEVER share this file
- ?? Treat it like a password!

Example path: `C:\Users\YourName\secure-vault-app-key.json`

---

## ? STEP 5: Set Environment Variable (CRITICAL!)

This tells your C# app where to find the credentials.

### For Windows:

**Method 1: Using GUI**
1. Press **Win + X**
2. Select **System**
3. Click **Advanced system settings** (left side)
4. Click **Environment Variables...** button (bottom-right)
5. Under "User variables for [YourName]", click **New...**
6. **Variable name**: `GOOGLE_APPLICATION_CREDENTIALS`
7. **Variable value**: `C:\Users\YourName\secure-vault-app-key.json`
   (Use the full path where you saved the JSON file)
8. Click **OK** ? **OK** ? **OK**

**Method 2: Using PowerShell (Admin)**
```powershell
$env:GOOGLE_APPLICATION_CREDENTIALS = "C:\Users\YourName\secure-vault-app-key.json"
```

### ?? CRITICAL: Restart Visual Studio!
After setting the environment variable, **completely close and reopen Visual Studio**.

The app reads the environment variable when it starts.

---

## ? STEP 6: Test the Setup

### 6.1 In Visual Studio:
1. Set **SecureVaultUI** as Startup Project
2. Press **F5** to run

### 6.2 In the App:
1. Enter your **Project ID** (from Step 1.6)
   - Example: `securevault-12345`
2. Click **"Connect"**
3. Should show: **"Successfully connected to GCP Secret Manager"** ?

### 6.3 Test Store:
1. **Secret Name**: `test-secret`
2. **Secret Value**: `TestValue123`
3. Click **"Store Secret"**
4. Should show: **"Secret 'test-secret' stored successfully!"** ?

### 6.4 Test Retrieve:
1. **Secret Name**: `test-secret`
2. Click **"Retrieve Secret"**
3. Should display: `TestValue123` ?

---

## ? STEP 7: Verify in GCP Console

### 7.1 Go back to GCP Console
```
https://console.cloud.google.com
```

### 7.2 Make sure your project is selected
Top blue bar: `SecureVault`

### 7.3 Search for "Secret Manager"
In the search bar, type: `Secret Manager`

### 7.4 Click "Secret Manager"
In the results.

### 7.5 You should see:
```
Secrets:
??? test-secret
    ??? Latest version: TestValue123
```

**If you see this, everything worked!** ?

---

## ?? Troubleshooting

### "Failed to connect to GCP Secret Manager"
**Solutions:**
- [ ] Verify environment variable is set: `echo %GOOGLE_APPLICATION_CREDENTIALS%`
- [ ] Restart Visual Studio (very important!)
- [ ] Check JSON file path is correct
- [ ] Verify JSON file exists and is readable

### "Permission denied"
**Solutions:**
- [ ] Go back to Service Account (Step 3)
- [ ] Verify it has "Secret Manager Admin" role
- [ ] Try creating a new key (Step 4)

### "Secret not found when retrieving"
**Solutions:**
- [ ] Make sure you stored it first
- [ ] Check the secret name (case-sensitive)
- [ ] Verify in GCP Console that the secret exists

---

## ?? You're Done!

Once you see the test work, you have a fully functioning GCP Secret Manager integration!

### Next Steps:
1. Read the code in each project folder
2. Understand the IKeyVault interface design
3. Consider implementing AWS/Azure support
4. Use in your own projects

---

## ?? Quick Reference

| Item | Value |
|------|-------|
| GCP Console | https://console.cloud.google.com |
| Service Account Name | secure-vault-app |
| Role | Secret Manager Admin |
| Environment Variable | GOOGLE_APPLICATION_CREDENTIALS |
| Key File | secure-vault-app-xxxxx.json |
| Project ID | (from Step 1.6) |

---

## ? Security Reminders

- ? Keep JSON key file safe
- ? Add `*.json` to .gitignore
- ? Never commit credentials
- ? Rotate keys periodically
- ? Use minimal permissions (Admin role is for testing, use lower permissions in production)

---

**You're all set! Follow these steps and your app will connect to GCP Secret Manager!** ??
