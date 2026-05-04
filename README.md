# Secure Vault - GCP Secret Manager Integration

A complete C# solution demonstrating how to use Google Cloud Platform (GCP) Secret Manager for secure storage and retrieval of secrets (AES-256 keys, credentials, etc.) with a clean architecture using interface abstraction.

## Features

? **GCP Secret Manager Integration** - Securely store and retrieve secrets in Google Cloud  
? **Interface-Based Architecture** - Easily swap implementations (GCP, AWS, Azure, DPAPI)  
? **AES-256 Key Management** - Generate, encrypt, and decrypt keys  
? **WPF Desktop UI** - User-friendly application for managing secrets  
? **REST API** - Production-ready Web API with best practices  
? **Async/Await Pattern** - Modern C# asynchronous operations  
? **Error Handling** - Comprehensive error handling and user feedback  
? **Server Deployment Ready** - Complete guide for Windows Server deployment with service accounts  
? **Automated Setup Scripts** - PowerShell scripts for easy credential configuration  

## Documentation

| Document | Purpose |
|----------|---------|
| **[GETTING_STARTED.md](GETTING_STARTED.md)** | Quick start guide for development |
| **[GCP_SETUP.md](GCP_SETUP.md)** | Detailed GCP configuration with automated setup script |
| **[CLONE_SETUP_GUIDE.md](CLONE_SETUP_GUIDE.md)** | Complete guide for cloning to a new computer |
| **[SERVER_DEPLOYMENT_GUIDE.md](SERVER_DEPLOYMENT_GUIDE.md)** | ??? **Server deployment instructions** |
| **[AUTHENTICATION_EXPLAINED.md](AUTHENTICATION_EXPLAINED.md)** | Understanding ADC vs Service Account authentication |
| **[SETUP_SCRIPT_REFERENCE.md](SETUP_SCRIPT_REFERENCE.md)** | Complete reference for setup-gcp-credentials.ps1 |
| **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** | Quick command reference and troubleshooting |

## Solution Structure

```
SecureVault.sln
??? SecureVaultInterface/       # Interface definitions
?   ??? IKeyVault.cs           # Core vault interface
??? SecureVaultCore/            # Shared utilities
?   ??? KeyManager.cs           # AES-256 key utilities
??? GcpSecretManagerVault/      # GCP implementation
?   ??? GcpSecretManagerVault.cs # GCP Secret Manager adapter
??? DpapiVault/                 # Windows DPAPI implementation
?   ??? DpapiVault.cs           # Local Windows vault
??? SecureVaultUI/              # WPF Desktop Application
?   ??? MainWindow.xaml         # UI layout
?   ??? MainWindow.xaml.cs      # UI logic
??? SecureVaultApi/             # ASP.NET Core Web API
    ??? Controllers/
    ?   ??? SecretsController.cs # REST endpoints
    ??? Services/
    ?   ??? VaultFactory.cs      # Provider factory
    ??? Models/                  # Request/Response models
    ??? appsettings.json         # Configuration
```

## Quick Start

### ?? Cloning to a New Computer?

**See [`CLONE_SETUP_GUIDE.md`](CLONE_SETUP_GUIDE.md)** for complete step-by-step instructions on setting up SecureVault on a new computer, including:
- Finding and copying the GCP service account key
- Setting the `GOOGLE_APPLICATION_CREDENTIALS` environment variable
- Automated setup script
- Troubleshooting guide

### ??? Deploying to a Server?

**See [`SERVER_DEPLOYMENT_GUIDE.md`](SERVER_DEPLOYMENT_GUIDE.md)** for complete server deployment instructions, including:
- Service account key setup for servers
- Setting environment variables (Machine-level)
- Windows Service installation
- IIS deployment
- Security best practices
- Firewall configuration
- Monitoring and troubleshooting

**Quick server setup:**
```powershell
# 1. Download service account key from GCP Console
# 2. Transfer securely to server
# 3. Set environment variable (as Administrator)
[System.Environment]::SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    "C:\SecureVault\.gcp\securevault-key.json",
    [System.EnvironmentVariableTarget]::Machine
)
# 4. Restart server
# 5. Deploy application
```

### Prerequisites

- .NET 8.0 or later
- Visual Studio 2022 or Visual Studio Code
- A free Google Cloud Platform account (for GCP provider)
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

## REST API Usage (Best Practice for Web Applications)

### Running the API

```bash
dotnet run --project SecureVaultApi
```

The API will be available at `https://localhost:5001` (or the port shown in console).

### Configuration

Edit `appsettings.json` to choose your vault provider:

```json
{
  "VaultSettings": {
    "Provider": "Dpapi",           // Options: "Gcp" or "Dpapi"
    "GcpProjectId": "your-project-id",  // Required if Provider is "Gcp"
    "DpapiVaultPath": ""           // Optional custom path for DPAPI
  }
}
```

**Best Practice:** The vault connection is created **once at application startup** and reused for all requests (singleton pattern).

### API Endpoints

#### 1. Get Single Secret

```http
GET /api/secrets/{id}
```

**Example:**
```bash
curl https://localhost:5001/api/secrets/database-password
```

**Response (200 OK):**
```json
{
  "id": "database-password",
  "value": "SuperSecure123!",
  "found": true
}
```

**Response (404 Not Found):**
```json
{
  "id": "non-existent-secret",
  "value": null,
  "found": false
}
```

#### 2. Get Multiple Secrets (Batch)

```http
POST /api/secrets/batch
Content-Type: application/json
```

**Request Body:**
```json
{
  "secretIds": ["database-password", "api-key", "jwt-secret"]
}
```

**Example:**
```bash
curl -X POST https://localhost:5001/api/secrets/batch \
  -H "Content-Type: application/json" \
  -d '{"secretIds":["database-password","api-key","jwt-secret"]}'
```

**Response (200 OK):**
```json
[
  {
    "id": "database-password",
    "value": "SuperSecure123!",
    "found": true
  },
  {
    "id": "api-key",
    "value": "abc123xyz",
    "found": true
  },
  {
    "id": "jwt-secret",
    "value": null,
    "found": false
  }
]
```

### Swagger UI

When running in Development mode, access interactive API documentation at:
```
https://localhost:5001/swagger
```

### REST API Best Practices Demonstrated

? **Singleton Connection Pattern** - Vault connects once at startup, not per request  
? **Async/Await** - All operations are fully asynchronous  
? **Concurrent Batch Retrieval** - Multiple secrets fetched in parallel  
? **Proper Error Handling** - Graceful degradation for missing secrets  
? **Structured Logging** - Comprehensive logging for monitoring  
? **Configuration-Based Provider** - Easily switch between GCP/DPAPI/AWS/Azure  
? **Swagger Documentation** - Auto-generated API docs  

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

### 4. DpapiVault
Windows DPAPI implementation of the `IKeyVault` interface.

**Key Types:**
- `DpapiVault` - Windows Data Protection API adapter

**Features:**
- Encrypted storage using Windows DPAPI
- User-scoped encryption (secrets only accessible by current user)
- File-based persistence
- No external dependencies or cloud services

**Use Case:**
- Development and testing
- Windows-only applications
- Local secret storage without cloud dependency

### 5. SecureVaultUI
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

### 6. SecureVaultApi (.NET 8 Web API)
Production-ready REST API demonstrating best practices for using vaults in web applications.

**Key Components:**
- `SecretsController` - REST endpoints for secret retrieval
- `VaultFactory` - Factory pattern for creating vault instances
- `Program.cs` - Dependency injection and singleton configuration

**Features:**
- Singleton vault connection (initialized at startup)
- Two REST endpoints: single secret and batch retrieval
- Configurable provider selection (GCP or DPAPI)
- Concurrent secret retrieval for batch operations
- Comprehensive error handling and logging
- Swagger/OpenAPI documentation
- Graceful shutdown with vault disconnection

**Best Practices:**
1. **Connect Once at Startup** - Vault connection is created during app initialization
2. **Reuse Connection** - Same connection used for all HTTP requests (singleton)
3. **Never Connect/Disconnect Per Request** - Avoids performance overhead
4. **Async Operations** - Fully async for scalability
5. **Configuration-Based** - Provider selection via appsettings.json

## How It Works

### Store Operation (UI Application)

1. User enters Secret Name and Value in UI
2. Application calls `vault.StoreAsync()`
3. Vault provider stores the secret (GCP or DPAPI)
4. A new secret version is created automatically (GCP)
5. User sees success confirmation

### Retrieve Operation (REST API)

1. HTTP request arrives at `/api/secrets/{id}`
2. Application uses the singleton vault instance (already connected)
3. Vault retrieves the secret value
4. JSON response returned to client
5. No connection/disconnection overhead per request

## Configuration

### Environment Variable (GCP Provider)

The application uses the standard GCP authentication via:

```
GOOGLE_APPLICATION_CREDENTIALS=C:\path\to\service-account-key.json
```

Set this environment variable to your service account key file path.

### REST API Configuration

In `SecureVaultApi/appsettings.json`:

```json
{
  "VaultSettings": {
    "Provider": "Dpapi",           // "Gcp" or "Dpapi"
    "GcpProjectId": "",            // Your GCP project ID (for GCP provider)
    "DpapiVaultPath": ""           // Optional: custom DPAPI vault path
  }
}
```

**To switch providers:**
1. Change `Provider` to `"Gcp"` or `"Dpapi"`
2. Set `GcpProjectId` if using GCP
3. Restart the API (connection happens at startup)

### Desktop UI Configuration

In the UI:
1. Enter your GCP Project ID (found in GCP Console)
2. Click "Connect"
3. Use the Store/Retrieve sections

## Extending the Solution

### Add AWS Secrets Manager Support

1. Create `AwsSecretsManagerVault` project
2. Implement `IKeyVault` interface
3. Use AWS SDK for .NET
4. Add to `VaultFactory.cs` in SecureVaultApi
5. Update configuration: `"Provider": "Aws"`

### Add Azure Key Vault Support

1. Create `AzureKeyVaultImpl` project
2. Implement `IKeyVault` interface
3. Use Azure Identity & Key Vault SDK
4. Add to `VaultFactory.cs` in SecureVaultApi
5. Update configuration: `"Provider": "Azure"`

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
- `Microsoft.AspNetCore.OpenApi` (for REST API)
- `Swashbuckle.AspNetCore` (for Swagger/OpenAPI documentation)
- Standard .NET libraries (no other external dependencies)

## Testing the Setup

### REST API Testing

```bash
# Start the API
dotnet run --project SecureVaultApi

# Store a secret using the UI first
dotnet run --project SecureVaultUI

# Then retrieve via REST API
curl https://localhost:5001/api/secrets/test-secret

# Batch retrieval
curl -X POST https://localhost:5001/api/secrets/batch \
  -H "Content-Type: application/json" \
  -d '{"secretIds":["secret1","secret2","secret3"]}'
```

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
