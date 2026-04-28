# GCP Secret Manager Setup Guide

This guide provides step-by-step instructions for setting up Google Cloud Platform (GCP) Secret Manager for use with the Secure Vault application.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Create a GCP Project](#create-a-gcp-project)
3. [Enable Secret Manager API](#enable-secret-manager-api)
4. [Create a Service Account](#create-a-service-account)
5. [Create and Download Service Account Key](#create-and-download-service-account-key)
6. [Configure Authentication](#configure-authentication)
7. [Test Your Setup](#test-your-setup)

---

## Prerequisites

Before you begin, ensure you have:
- A free Google Cloud Platform account (https://cloud.google.com/free)
- A web browser
- The C# application code from this repository
- .NET 6.0 or later installed on your machine

---

## Create a GCP Project

### Step 1.1: Sign In to GCP Console
1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Sign in with your Google account
3. You'll see the GCP dashboard

### Step 1.2: Create a New Project
1. Click the **Project Selector** at the top (shows "Select a Project")
2. Click **"NEW PROJECT"** button
3. Enter a **Project Name** (e.g., "SecureVault")
4. Leave the organization field as default (usually empty for free tier)
5. Click **"CREATE"**
6. Wait for the project to be created (takes a few seconds)
7. Once created, the new project will be automatically selected

### Step 1.3: Note Your Project ID
1. In the top blue bar, click the project selector again
2. Your project will be listed with:
   - **Project Name**: The name you entered
   - **Project ID**: A unique identifier (format: `projectname-12345`) securevault001
   - **Project Number**: A numeric ID
3. **Copy and save the Project ID** - you'll need this later in the application

---

## Enable Secret Manager API

### Step 2.1: Navigate to APIs
1. In the left sidebar, click **"APIs & Services"**
2. Click **"Enabled APIs & services"**

### Step 2.2: Enable Secret Manager API
1. Click the **"+ ENABLE APIS AND SERVICES"** button
2. Search for **"Secret Manager"** in the search box
3. Click on **"Google Secret Manager API"** in the results
4. Click the **"ENABLE"** button
5. Wait for the API to be enabled (takes a few seconds)
6. You should see a confirmation page with "Google Secret Manager API" enabled

---

## Create a Service Account

A service account is a special account used by applications to authenticate with GCP services.

### Step 3.1: Navigate to Service Accounts
1. In the left sidebar under "APIs & Services", click **"Credentials"**
2. Click the **"+ CREATE CREDENTIALS"** button
3. Select **"Service Account"**

### Step 3.2: Fill in Service Account Details
1. **Service Account Name**: Enter `secure-vault-app` (or any name you prefer)
2. **Service Account ID**: Auto-populated (you can leave as-is)
3. Click **"CREATE AND CONTINUE"**

### Step 3.3: Grant Permissions
1. You'll see "Grant this service account access to project"
2. Click the **"Select a role"** dropdown
3. Search for and select **"Secret Manager Admin"**
   - This role allows the service account to create, store, and retrieve secrets
4. Click **"CONTINUE"**
5. Click **"DONE"**

---

## Create and Download Service Account Key

### Step 4.1: Generate Private Key
1. You'll be back at the Credentials page
2. Under "Service Accounts", find the account you just created (`secure-vault-app`)
3. Click on the **service account name** (the email address)
4. Go to the **"KEYS"** tab at the top
5. Click **"Add Key"** ? **"Create new key"**
6. Choose **"JSON"** as the key type
7. Click **"CREATE"**
8. A JSON file will be automatically downloaded (e.g., `secure-vault-app-xxxxx.json`)

### Step 4.2: Important Security Notes
?? **SECURITY CRITICAL**
- This JSON file contains credentials that grant access to your GCP resources
- **Never commit this file to version control** (GitHub, git, etc.)
- **Never share this file publicly**
- Store it in a secure location on your computer
- You can regenerate keys if compromised

---

## Configure Authentication

The GCP SDK automatically discovers credentials from environment variables. You need to set the path to your service account key.

### Option A: Windows Environment Variable (Recommended for Development)

1. **Locate your JSON key file** from Step 4.2
   - Note the full file path, e.g., `C:\Users\YourName\Downloads\secure-vault-app-xxxxx.json`

2. **Set Environment Variable**:
   - Press **Win + X** and select **System**
   - Click **"Advanced system settings"** on the left
   - Click **"Environment Variables"** button
   - Under "User variables for [YourName]", click **"New..."**
   - **Variable name**: `GOOGLE_APPLICATION_CREDENTIALS`
   - **Variable value**: Paste the full path to your JSON key file
   - Click **"OK"** on all dialogs

3. **Restart Your Application**:
   - After setting the environment variable, you must close and reopen your C# application
   - The application will now automatically use the service account credentials

### Option B: Programmatic Configuration (Alternative)

If you prefer to configure credentials in code instead of environment variables:

```csharp
// In your MainWindow.xaml.cs, before creating the vault:
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", 
    @"C:\path\to\secure-vault-app-xxxxx.json");

_vault = new GcpSecretManagerVault(projectId);
await _vault.ConnectAsync();
```

---

## Test Your Setup

### Step 6.1: Using the Application

1. **Start the SecureVaultUI application**
2. **In the "GCP Configuration" section**:
   - Paste your **Project ID** into the "Project ID" field
   - Click **"Connect"**
3. **Test Storing a Secret**:
   - In "Store Secret" section:
   - Secret Name: `test-secret`
   - Secret Value: `MyTestValue123`
   - Click **"Store Secret"**
   - You should see: "Secret 'test-secret' stored successfully!"
4. **Test Retrieving a Secret**:
   - In "Retrieve Secret" section:
   - Secret Name: `test-secret`
   - Click **"Retrieve Secret"**
   - The "Retrieved Secret Value" box should show: `MyTestValue123`

### Step 6.2: Verify in GCP Console

1. Go to [GCP Console](https://console.cloud.google.com)
2. Make sure your project is selected (top bar)
3. In the left sidebar, search for **"Secret Manager"**
4. Click **"Secret Manager"**
5. You should see your `test-secret` listed
6. Click on it to view details and versions

---

## Troubleshooting

### Error: "Failed to connect to GCP Secret Manager"

**Possible causes:**

1. **Environment variable not set**
   - Verify `GOOGLE_APPLICATION_CREDENTIALS` is set correctly
   - Remember to restart your application after setting it
   - Check the path is correct and file exists

2. **Secret Manager API not enabled**
   - Go to APIs & Services ? Enabled APIs & services
   - Search for "Secret Manager"
   - Make sure it's enabled

3. **Credentials file not found**
   - Verify the JSON key file path is correct
   - Make sure the file has read permissions

### Error: "Permission denied"

**Causes:**

1. **Service account doesn't have proper role**
   - Go to IAM & Admin ? Service Accounts
   - Select your service account
   - Go to Permissions tab
   - Make sure it has "Secret Manager Admin" role

### Error: "Not found"

**Causes:**

1. **Secret doesn't exist in the vault**
   - Retrieve uses only secrets you've previously stored
   - Check the exact name (case-sensitive)
   - Verify in GCP Console that the secret exists

---

## Using the Code

### Architecture Overview

The solution is organized into 4 projects:

1. **SecureVaultInterface** - `IKeyVault` interface
   - Defines the contract for any key vault implementation
   - Methods: `ConnectAsync()`, `DisconnectAsync()`, `StoreAsync()`, `RetrieveAsync()`

2. **SecureVaultCore** - Utility classes
   - `KeyManager.cs` - AES-256 encryption utilities
   - Can generate keys, encrypt, decrypt data

3. **GcpSecretManagerVault** - GCP Implementation
   - `GcpSecretManagerVault.cs` - Implements `IKeyVault` using GCP Secret Manager

4. **SecureVaultUI** - WPF Desktop Application
   - `MainWindow.xaml` - UI layout
   - `MainWindow.xaml.cs` - Logic for Store/Retrieve operations

### Example Usage (Code)

```csharp
using SecureVaultInterface;
using GcpSecretManagerVault;

// Create vault instance
var vault = new GcpSecretManagerVault("your-project-id");

// Connect to GCP
await vault.ConnectAsync();

// Store a secret
await vault.StoreAsync("my-aes-key", "mysecretvalue123");

// Retrieve a secret
string? secret = await vault.RetrieveAsync("my-aes-key");
Console.WriteLine($"Retrieved: {secret}");

// Disconnect
await vault.DisconnectAsync();
```

---

## Future Enhancements

The interface-based design allows you to implement additional vault providers:

```csharp
// Example: AWS Secrets Manager implementation
public class AwsSecretsManagerVault : IKeyVault { ... }

// Example: Azure Key Vault implementation
public class AzureKeyVaultImpl : IKeyVault { ... }

// Example: DPAPI implementation (Windows-only)
public class DpapiVault : IKeyVault { ... }

// Usage remains the same
IKeyVault vault = new AwsSecretsManagerVault();
await vault.ConnectAsync();
```

---

## Security Best Practices

1. **Never commit credentials to version control**
   - Add `*.json` key files to `.gitignore`

2. **Use service accounts, not user credentials**
   - This is already done in the setup above

3. **Rotate keys periodically**
   - In GCP Service Accounts ? Keys ? Manage keys
   - Delete old keys and create new ones

4. **Use minimal permissions**
   - The setup uses "Secret Manager Admin" which is appropriate
   - For read-only operations, use "Secret Manager Secret Accessor"

5. **Store sensitive data securely**
   - Never hardcode Project IDs or secret values
   - Use configuration files or environment variables

6. **Audit access**
   - Enable Cloud Audit Logs in GCP
   - Periodically review who accessed which secrets

---

## Additional Resources

- [GCP Secret Manager Documentation](https://cloud.google.com/secret-manager/docs)
- [GCP Service Accounts](https://cloud.google.com/iam/docs/service-accounts)
- [Google Cloud C# Libraries](https://github.com/googleapis/google-cloud-dotnet)
- [Secret Manager API Reference](https://cloud.google.com/secret-manager/docs/reference/rest)

---

## Support

If you encounter issues:

1. Check the Troubleshooting section above
2. Verify each setup step was completed
3. Check GCP Console to confirm resources are created
4. Ensure environment variable is set and application is restarted
5. Review GCP IAM logs for permission issues

Good luck with your secure vault application! ??
