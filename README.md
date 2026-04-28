# Secure Vault - GCP Secret Manager Integration

A complete C# solution demonstrating how to use Google Cloud Platform (GCP) Secret Manager for secure storage and retrieval of secrets (AES-256 keys, credentials, etc.) with a clean architecture using interface abstraction.

## Features

? **GCP Secret Manager Integration** - Securely store and retrieve secrets in Google Cloud  
? **Interface-Based Architecture** - Easily swap implementations (GCP, AWS, Azure, DPAPI)  
? **AES-256 Key Management** - Generate, encrypt, and decrypt keys  
? **WPF Desktop UI** - User-friendly application for managing secrets  
? **Async/Await Pattern** - Modern C# asynchronous operations  
? **Error Handling** - Comprehensive error handling and user feedback  

## Solution Structure

```
SecureVault.sln
??? SecureVaultInterface/       # Interface definitions
?   ??? IKeyVault.cs           # Core vault interface
??? SecureVaultCore/            # Shared utilities
?   ??? KeyManager.cs           # AES-256 key utilities
??? GcpSecretManagerVault/       # GCP implementation
?   ??? GcpSecretManagerVault.cs # GCP Secret Manager adapter
??? SecureVaultUI/              # WPF Desktop Application
    ??? MainWindow.xaml         # UI layout
    ??? MainWindow.xaml.cs      # UI logic
    ??? App.xaml & App.xaml.cs  # Application entry point
```

## Quick Start

### Prerequisites

- .NET 6.0 or later
- Visual Studio 2022 or Visual Studio Code
- A free Google Cloud Platform account
- Google Cloud CLI (optional, for advanced management)

### Setup Steps

1. **Clone/Open the Solution**
   ```bash
   cd C:\Users\albertly\source\repos\albert0ly
   ```

2. **Follow GCP Setup Guide** (VERY IMPORTANT!)
   - Read `GCP_SETUP.md` for complete step-by-step instructions
   - Create a GCP project
   - Enable Secret Manager API
   - Create a service account
   - Download and configure credentials

3. **Build the Solution**
   ```bash
   dotnet build
   ```

4. **Run the Application**
   ```bash
   dotnet run --project SecureVaultUI
   ```

5. **Use the UI**
   - Enter your GCP Project ID
   - Click "Connect"
   - Store and retrieve secrets

## API Usage

### Interface Definition

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

### Example Code

```csharp
using SecureVaultInterface;
using GcpSecretManagerVault;

// Create vault instance
var vault = new GcpSecretManagerVault("your-gcp-project-id");

// Connect to GCP Secret Manager
await vault.ConnectAsync();

// Store a secret
await vault.StoreAsync("database-password", "SuperSecure123!");

// Retrieve a secret
string? password = await vault.RetrieveAsync("database-password");

// Disconnect
await vault.DisconnectAsync();
```

## Projects Overview

### 1. SecureVaultInterface
Defines the `IKeyVault` interface that all vault implementations must follow.

**Key Types:**
- `IKeyVault` - Interface with Connect/Disconnect/Store/Retrieve methods

**Use Case:**
- Abstraction layer allowing multiple implementations
- Easy to swap between GCP, AWS, Azure, DPAPI, etc.

### 2. SecureVaultCore
Utility classes for cryptographic operations.

**Key Types:**
- `KeyManager` - AES-256 key generation, encryption, decryption

**Methods:**
- `GenerateAes256Key()` - Generate a new AES-256 key
- `GenerateAes256IV()` - Generate initialization vector
- `EncryptAes256()` - Encrypt data with AES-256
- `DecryptAes256()` - Decrypt data with AES-256

### 3. GcpSecretManagerVault
GCP Secret Manager implementation of the `IKeyVault` interface.

**Key Types:**
- `GcpSecretManagerVault` - GCP Secret Manager adapter

**Features:**
- Automatic secret creation if it doesn't exist
- Support for secret versioning
- Proper error handling for GCP API calls
- Connection management

**Dependencies:**
- `Google.Cloud.SecretManager.V1` (NuGet package)

### 4. SecureVaultUI
WPF desktop application providing a user interface.

**Key Components:**
- `MainWindow.xaml` - UI layout with sections for:
  - Connection Status
  - GCP Project ID input
  - Store Secret form
  - Retrieve Secret form
- `MainWindow.xaml.cs` - Event handlers and logic

**Features:**
- Real-time connection status indicator
- Async operations with proper UI feedback
- Error display and messaging
- Input validation

## GCP Setup (Quick Checklist)

See `GCP_SETUP.md` for detailed instructions. Quick checklist:

- [ ] Create GCP Project
- [ ] Enable Secret Manager API
- [ ] Create Service Account with "Secret Manager Admin" role
- [ ] Download service account JSON key
- [ ] Set `GOOGLE_APPLICATION_CREDENTIALS` environment variable
- [ ] Restart application
- [ ] Test Store/Retrieve operations

## Configuration

### Environment Variable

The application uses the standard GCP authentication via:

```
GOOGLE_APPLICATION_CREDENTIALS=C:\path\to\service-account-key.json
```

Set this environment variable to your service account key file path.

### Application Configuration

In the UI:
1. Enter your GCP Project ID (found in GCP Console)
2. Click "Connect"
3. Use the Store/Retrieve sections

## How It Works

### Store Operation

1. User enters Secret Name and Value in UI
2. Application calls `vault.StoreAsync()`
3. GCP Secret Manager creates or updates the secret
4. A new secret version is created automatically
5. User sees success confirmation

### Retrieve Operation

1. User enters Secret Name in UI
2. Application calls `vault.RetrieveAsync()`
3. GCP retrieves the latest version of the secret
4. Secret value is displayed in the output field
5. User sees success confirmation

## Extending the Solution

### Add AWS Secrets Manager Support

1. Create `AwsSecretsManagerVault` project
2. Implement `IKeyVault` interface
3. Use AWS SDK for .NET
4. Reference in UI project
5. Change instantiation: `IKeyVault vault = new AwsSecretsManagerVault();`

### Add Azure Key Vault Support

1. Create `AzureKeyVaultImpl` project
2. Implement `IKeyVault` interface
3. Use Azure Identity & Key Vault SDK
4. Reference in UI project
5. Change instantiation: `IKeyVault vault = new AzureKeyVaultImpl();`

### Add Local DPAPI Support

1. Create `DpapiVault` project
2. Implement `IKeyVault` interface
3. Use `System.Security.Cryptography.DataProtectionScope`
4. Perfect for Windows-only applications
5. No external dependencies needed

## Security Notes

?? **Important Security Considerations:**

1. **Never commit credentials** to version control
   - Add `*.json` to `.gitignore`
   - Add `GOOGLE_APPLICATION_CREDENTIALS` to secrets management

2. **Service Account Keys are sensitive**
   - Treat like passwords
   - Regenerate periodically
   - Revoke if compromised

3. **Use minimal permissions**
   - Service account only needs "Secret Manager Admin" for this app
   - Use "Secret Manager Secret Accessor" for read-only applications

4. **Audit Access**
   - Enable Cloud Audit Logs in GCP
   - Monitor who accesses which secrets
   - Review access patterns regularly

5. **Environment Variable Security**
   - Store key file in secure location
   - Restrict file permissions (Windows ACLs)
   - Consider alternative auth methods in production

## Troubleshooting

### Connection Failed

1. Verify `GOOGLE_APPLICATION_CREDENTIALS` is set
2. Restart the application after setting environment variable
3. Verify service account key file exists and is readable
4. Check Project ID is correct (in GCP Console)

### Permission Denied

1. Verify service account has "Secret Manager Admin" role
2. Check role assignment in GCP IAM console
3. Try regenerating service account key

### Secret Not Found

1. Verify secret name is spelled correctly (case-sensitive)
2. Check secret exists in GCP Secret Manager console
3. Ensure you stored it first before retrieving

## NuGet Dependencies

- `Google.Cloud.SecretManager.V1` (for GCP implementation)
- Standard .NET libraries (no other external dependencies)

## Testing the Setup

### Unit Test Example

```csharp
[Test]
public async Task TestStoreAndRetrieve()
{
    var vault = new GcpSecretManagerVault("test-project-id");
    await vault.ConnectAsync();
    
    await vault.StoreAsync("test-key", "test-value");
    var result = await vault.RetrieveAsync("test-key");
    
    Assert.AreEqual("test-value", result);
}
```

## License

This sample project is provided as-is for educational purposes.

## Support

For issues:
1. Review `GCP_SETUP.md` for configuration help
2. Check GCP Console for secret creation
3. Verify environment variables are set
4. Review error messages in application UI

---

**Ready to secure your secrets? Start with `GCP_SETUP.md`!** ??
