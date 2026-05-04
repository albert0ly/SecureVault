# ?? Secure Vault - GCP Secret Manager Sample App

**Location**: `C:\Users\albertly\source\repos\SecureVault`

A complete C# WPF application demonstrating secure secret management using Google Cloud Platform (GCP) Secret Manager.

## ?? Project Structure

```
C:\Users\albertly\source\repos\SecureVault\
??? SecureVault.sln                    # Main solution file
??? GCP_SETUP.md                       # Detailed GCP configuration guide
??? README.md                          # Project documentation
??? GETTING_STARTED.md                 # This file
?
??? SecureVaultInterface/              # Interface abstraction layer
?   ??? SecureVaultInterface.csproj
?   ??? IKeyVault.cs                   # Core vault interface definition
?
??? SecureVaultCore/                   # Shared utilities
?   ??? SecureVaultCore.csproj
?   ??? KeyManager.cs                  # AES-256 encryption utilities
?
??? GcpSecretManagerVault/             # GCP implementation
?   ??? GcpSecretManagerVault.csproj
?   ??? GcpSecretManagerVault.cs       # GCP Secret Manager adapter
?
??? SecureVaultUI/                     # WPF Desktop Application
    ??? SecureVaultUI.csproj
    ??? App.xaml & App.xaml.cs         # Application entry point
    ??? MainWindow.xaml                # UI layout
    ??? MainWindow.xaml.cs             # UI logic and event handlers
```

## ?? Quick Start (5 Minutes)

### 1. Open the Solution in Visual Studio

```
Visual Studio ? File ? Open ? Browse to:
C:\Users\albertly\source\repos\SecureVault\SecureVault.sln
```

Or from command line:
```powershell
cd C:\Users\albertly\source\repos\SecureVault
start SecureVault.sln
```

### 2. Verify Build Status

The solution should build without errors:
```powershell
cd C:\Users\albertly\source\repos\SecureVault
dotnet build SecureVault.sln
```

Expected output:
```
SecureVaultInterface -> ...SecureVaultInterface.dll
SecureVaultCore -> ...SecureVaultCore.dll
GcpSecretManagerVault -> ...GcpSecretManagerVault.dll
SecureVaultUI -> ...SecureVaultUI.dll

Build succeeded.
```

### 3. Set Up GCP (Required Before Running)

**See `GCP_SETUP.md` for detailed instructions**, but quick summary:

1. ? Create GCP Project at https://console.cloud.google.com
2. ? Enable Secret Manager API
3. ? Create Service Account with "Secret Manager Admin" role
4. ? Download service account key (JSON file)
5. ? Set environment variable:
   ```
   GOOGLE_APPLICATION_CREDENTIALS=C:\path\to\your\key.json
   ```
6. ? Restart Visual Studio or application

### 4. Run the Application

In Visual Studio:
- **Right-click** on `SecureVaultUI` project
- Select **Set as Startup Project**
- Press **F5** to run

Or from command line:
```powershell
cd C:\Users\albertly\source\repos\SecureVault
dotnet run --project SecureVaultUI
```

### 5. Test It!

1. **Enter your GCP Project ID** (from `GCP_SETUP.md`)
2. **Click "Connect"** - should show green status
3. **Store a secret**:
   - Secret Name: `test-secret`
   - Value: `TestValue123`
   - Click **Store Secret**
4. **Retrieve it**:
   - Secret Name: `test-secret`
   - Click **Retrieve Secret**
   - Should display: `TestValue123`

## ?? Project Overview

### SecureVaultInterface
Defines the abstraction for key vault operations. Allows different implementations (GCP, AWS, Azure, DPAPI).

```csharp
public interface IKeyVault
{
    Task ConnectAsync();
    Task DisconnectAsync();
    Task StoreAsync(string secretName, string secretValue);
    Task<string?> RetrieveAsync(string secretName);
    bool IsConnected { get; }
}
```

### SecureVaultCore
Provides AES-256 encryption utilities for client-side key management.

**Key Methods**:
- `GenerateAes256Key()` - Creates a new 256-bit key
- `GenerateAes256IV()` - Creates initialization vector
- `EncryptAes256()` - Encrypts data
- `DecryptAes256()` - Decrypts data

### GcpSecretManagerVault
Implementation of `IKeyVault` using Google Cloud Secret Manager.

**Features**:
- Automatic secret creation
- Version management
- Async/await support
- Proper error handling

**NuGet Packages**:
- `Google.Cloud.SecretManager.V1` v2.3.0
- `Grpc.Core` v2.46.5

### SecureVaultUI
WPF desktop application with user-friendly interface.

**Features**:
- Real-time connection status
- Store secrets
- Retrieve secrets
- Error messages and feedback
- Input validation

## ?? Configuration

### Automated Setup Script (Recommended)

The easiest way to set up GCP credentials is to use the automated setup script:

```powershell
# Run the setup script
.\setup-gcp-credentials.ps1
```

**What the script does:**
1. ? Creates secure credentials directory (`%USERPROFILE%\.gcp`)
2. ? Guides you through downloading the GCP service account key
3. ? Searches for JSON key files in common locations
4. ? Validates the key file exists
5. ? Sets the `GOOGLE_APPLICATION_CREDENTIALS` environment variable
6. ? Verifies the setup

**Script features:**
- Interactive prompts
- Automatic error checking
- Colored output for easy reading
- Validates file paths
- Sets environment variable permanently

**After running the script:**
- Restart Visual Studio or PowerShell
- Verify by running: `$env:GOOGLE_APPLICATION_CREDENTIALS`

---

### Manual Environment Variable Setup (Alternative)

If you prefer to set up manually:

#### Option 1: Windows GUI

1. Press **Win + X** ? Select **System**
2. Click **Advanced system settings**
3. Click **Environment Variables...**
4. Under "User variables", click **New...**
5. Variable name: `GOOGLE_APPLICATION_CREDENTIALS`
6. Variable value: `C:\Users\<username>\.gcp\securevault-key.json`
7. Click **OK** ? **OK** ? **OK**
8. **Restart Visual Studio** (important!)

#### Option 2: PowerShell Command

```powershell
# Set the environment variable permanently
setx GOOGLE_APPLICATION_CREDENTIALS "C:\Users\<username>\.gcp\securevault-key.json"

# Restart PowerShell or Visual Studio after this
```

### Verify Configuration

After setup, verify the configuration:

```powershell
# Check environment variable
$env:GOOGLE_APPLICATION_CREDENTIALS

# Verify file exists
Test-Path $env:GOOGLE_APPLICATION_CREDENTIALS

# Run verification script
.\verify-setup.ps1
```

Expected output:
```
C:\Users\albertly\.gcp\securevault-key.json
True
? ALL CHECKS PASSED!
```

## ?? Common Tasks

### Adding AWS Secrets Manager Support

1. Create new project: `AwsSecretsManagerVault`
2. Reference `SecureVaultInterface`
3. Implement `IKeyVault` interface
4. Add to `SecureVaultUI` project reference
5. Update MainWindow.cs to use new implementation

```csharp
// Instead of:
IKeyVault vault = new GcpSecretManagerVault(projectId);

// Use:
IKeyVault vault = new AwsSecretsManagerVault(region, accessKey, secretKey);
```

### Storing Encrypted Keys

```csharp
// Generate a key
string newKey = KeyManager.GenerateAes256Key();
string newIv = KeyManager.GenerateAes256IV();

// Store in vault
await vault.StoreAsync("my-encryption-key", newKey);
await vault.StoreAsync("my-encryption-iv", newIv);

// Retrieve and use
string key = await vault.RetrieveAsync("my-encryption-key");
string iv = await vault.RetrieveAsync("my-encryption-iv");
string encrypted = KeyManager.EncryptAes256(plaintext, key, iv);
```

## ?? Troubleshooting

### Connection Failed

**Problem**: "Failed to connect to GCP Secret Manager"

**Solutions**:
- [ ] Verify `GOOGLE_APPLICATION_CREDENTIALS` is set
- [ ] Restart Visual Studio/application after setting env var
- [ ] Check JSON key file exists and is readable
- [ ] Verify Project ID is correct
- [ ] Confirm Secret Manager API is enabled in GCP Console

### Permission Denied

**Problem**: "Permission denied" error when storing/retrieving

**Solutions**:
- [ ] Verify service account has "Secret Manager Admin" role
- [ ] Check in GCP Console: IAM & Admin ? Roles
- [ ] Try regenerating service account key

### Secret Not Found

**Problem**: Retrieve returns nothing

**Solutions**:
- [ ] Verify secret was stored first
- [ ] Check secret name (case-sensitive)
- [ ] Verify in GCP Console: Secret Manager
- [ ] Ensure it's in the same project

### Build Errors

**Problem**: CS0118 namespace/type conflict

**Solution**: This has been fixed with alias:
```csharp
using GcpVault = GcpSecretManagerVault.GcpSecretManagerVault;
```

## ?? Next Steps

1. **Read**: `GCP_SETUP.md` - Complete GCP configuration
2. **Read**: `README.md` - Full project documentation
3. **Explore**: Code in each project folder
4. **Extend**: Implement AWS or Azure providers
5. **Integrate**: Use in your own projects

## ?? Security Best Practices

? **Do**:
- Store credentials in environment variables
- Use service accounts (not user credentials)
- Keep JSON key files in `.gitignore`
- Rotate keys periodically
- Use minimal required permissions
- Enable GCP Audit Logs

? **Don't**:
- Commit JSON key files to Git
- Hardcode Project IDs
- Share key files publicly
- Use the same key for all environments
- Log sensitive values

## ?? Support Resources

- **GCP Documentation**: https://cloud.google.com/secret-manager/docs
- **Google Cloud .NET Libraries**: https://github.com/googleapis/google-cloud-dotnet
- **Service Accounts**: https://cloud.google.com/iam/docs/service-accounts
- **Secret Manager API**: https://cloud.google.com/secret-manager/docs/reference/rest

## ? Architecture

```
???????????????????????????????????????????
?       SecureVaultUI (WPF App)          ?
?  ???????????????????????????????????   ?
?  ? MainWindow                      ?   ?
?  ? - Connect Button                ?   ?
?  ? - Store/Retrieve UI             ?   ?
?  ? - Status Display                ?   ?
?  ???????????????????????????????????   ?
???????????????????????????????????????????
                ?
                ?
        ?????????????????
        ?  IKeyVault    ? (Interface)
        ?????????????????
                ?
        ???????????????????
        ?                 ?
  ????????????????  ???????????????????
  ? GcpVault     ?  ? Future: AwsVault?
  ? Impl.        ?  ? AzureVault, etc.?
  ????????????????  ???????????????????
         ?
         ?
    ???????????????????????????????????
    ?  SecureVaultCore                ?
    ?  - KeyManager (AES-256)         ?
    ?  - Encryption utilities         ?
    ???????????????????????????????????
         ?
         ?
    ???????????????????????????????????
    ?  Google Cloud Secret Manager    ?
    ?  - Store secrets                ?
    ?  - Retrieve with versioning     ?
    ???????????????????????????????????
```

## ?? Solution File Mapping

```
SecureVault.sln
??? SecureVaultInterface (net8.0)
?   ??? Defines IKeyVault interface
?
??? SecureVaultCore (net8.0)
?   ??? References: SecureVaultInterface
?   ??? KeyManager utilities
?
??? GcpSecretManagerVault (net8.0)
?   ??? References: SecureVaultInterface
?   ??? NuGet: Google.Cloud.SecretManager.V1
?   ??? NuGet: Grpc.Core
?   ??? GCP implementation
?
??? SecureVaultUI (net8.0-windows WPF)
    ??? References: SecureVaultInterface, SecureVaultCore, GcpSecretManagerVault
    ??? MainWindow.xaml
    ??? App.xaml
```

---

**Ready to go!** ?? Start with opening the solution in Visual Studio, then follow the GCP setup guide.
