# SecureVault REST API - Best Practices Guide

## Overview

The SecureVault REST API demonstrates production-ready best practices for using secret vaults in web applications. This guide explains the architecture, configuration, and usage patterns.

## Architecture Best Practices

### ? Singleton Connection Pattern (Recommended)

The API connects to the vault **once at application startup** and reuses the connection for all requests:

```csharp
// In Program.cs - runs once at startup
services.AddSingleton<IKeyVault>(serviceProvider =>
{
    var vault = VaultFactory.CreateVault(configuration);
    vault.ConnectAsync().GetAwaiter().GetResult(); // Connect at startup
    return vault;
});
```

**Why This Is Best:**
- ? **Fast**: No connection overhead per request (0ms vs 300-1000ms)
- ? **Efficient**: Reuses authentication tokens and network connections
- ? **Scalable**: No rate limiting issues from cloud providers
- ? **Simple**: Clean dependency injection pattern

### ? Anti-Patterns to Avoid

**DO NOT** connect/disconnect on every request:

```csharp
// BAD - Don't do this!
public async Task<IActionResult> GetSecret(string id)
{
    await _vault.ConnectAsync();      // Slow!
    var secret = await _vault.RetrieveAsync(id);
    await _vault.DisconnectAsync();   // Wasteful!
    return Ok(secret);
}
```

**Why This Is Bad:**
- ? 300-1000ms overhead per request
- ? May hit API rate limits
- ? Wastes CPU and network resources
- ? Scales poorly under load

## Configuration

### Provider Selection

Edit `appsettings.json` to choose between DPAPI (local) or GCP (cloud):

```json
{
  "VaultSettings": {
    "Provider": "Dpapi",              // Options: "Gcp" or "Dpapi"
    "GcpProjectId": "",               // Required for GCP provider
    "DpapiVaultPath": ""              // Optional: custom DPAPI path
  }
}
```

### Environment-Specific Configuration

- `appsettings.json` - Default configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides

### GCP Provider Setup

1. Set environment variable:
   ```bash
   set GOOGLE_APPLICATION_CREDENTIALS=C:\path\to\service-account.json
   ```

2. Update `appsettings.json`:
   ```json
   {
     "VaultSettings": {
       "Provider": "Gcp",
       "GcpProjectId": "your-gcp-project-id"
     }
   }
   ```

3. Restart the API

### DPAPI Provider Setup

1. Update `appsettings.json`:
   ```json
   {
     "VaultSettings": {
       "Provider": "Dpapi",
       "DpapiVaultPath": ""  // Uses default AppData location
     }
   }
   ```

2. Store secrets using the SecureVaultUI desktop app first

## API Endpoints

### 1. Get Single Secret

Retrieves a single secret by its identifier.

**Endpoint:**
```
GET /api/secrets/{id}
```

**Parameters:**
- `id` (path) - The secret identifier/name

**Example Request:**
```bash
curl https://localhost:5001/api/secrets/database-password
```

**Success Response (200 OK):**
```json
{
  "id": "database-password",
  "value": "SuperSecure123!",
  "found": true
}
```

**Not Found Response (404):**
```json
{
  "id": "non-existent-secret",
  "value": null,
  "found": false
}
```

**Error Response (500):**
```json
{
  "error": "Failed to retrieve secret",
  "message": "Connection timeout"
}
```

### 2. Get Multiple Secrets (Batch)

Retrieves multiple secrets in a single request with concurrent processing.

**Endpoint:**
```
POST /api/secrets/batch
```

**Request Body:**
```json
{
  "secretIds": ["secret1", "secret2", "secret3"]
}
```

**Example Request:**
```bash
curl -X POST https://localhost:5001/api/secrets/batch \
  -H "Content-Type: application/json" \
  -d '{
    "secretIds": [
      "database-password",
      "api-key",
      "jwt-secret"
    ]
  }'
```

**Success Response (200 OK):**
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

**Features:**
- ? Concurrent retrieval for better performance
- ? Returns all requested secrets (even if some are not found)
- ? Individual error handling per secret

## Running the API

### Development Mode

```bash
cd SecureVaultApi
dotnet run
```

Access Swagger UI at: `https://localhost:5001/swagger`

### Production Mode

```bash
dotnet run --environment Production
```

## Swagger/OpenAPI Documentation

Interactive API documentation is available in Development mode:

1. Start the API: `dotnet run --project SecureVaultApi`
2. Open browser: `https://localhost:5001/swagger`
3. Test endpoints directly from the UI

## Testing Workflow

### Step 1: Store Secrets (Using Desktop UI)

```bash
# Run the WPF UI application
dotnet run --project SecureVaultUI

# Store some test secrets:
# - Name: database-password, Value: SuperSecure123!
# - Name: api-key, Value: abc123xyz
# - Name: jwt-secret, Value: my-jwt-secret
```

### Step 2: Retrieve Secrets (Using REST API)

```bash
# Start the REST API
dotnet run --project SecureVaultApi

# Test single secret retrieval
curl https://localhost:5001/api/secrets/database-password

# Test batch retrieval
curl -X POST https://localhost:5001/api/secrets/batch \
  -H "Content-Type: application/json" \
  -d '{"secretIds":["database-password","api-key","jwt-secret"]}'
```

## Performance Characteristics

### Singleton Connection (Current Implementation)

| Metric | Value |
|--------|-------|
| First request latency | 50-100ms |
| Subsequent requests | 5-20ms |
| Throughput | High (limited by vault service) |
| Resource usage | Low |

### Per-Request Connection (Anti-Pattern)

| Metric | Value |
|--------|-------|
| Every request latency | 300-1000ms |
| Throughput | Low |
| Resource usage | High |
| Rate limit risk | High |

## Extending the API

### Adding New Vault Providers

1. **Create Implementation:**
   ```csharp
   // AwsSecretsManagerVault/AwsSecretsManagerVault.cs
   public class AwsSecretsManagerVault : IKeyVault
   {
       // Implement interface
   }
   ```

2. **Update VaultFactory:**
   ```csharp
   public static IKeyVault CreateVault(IConfiguration configuration)
   {
       var provider = configuration["VaultSettings:Provider"]?.ToLowerInvariant();
       
       return provider switch
       {
           "gcp" => CreateGcpVault(configuration),
           "dpapi" => CreateDpapiVault(configuration),
           "aws" => CreateAwsVault(configuration),  // Add this
           _ => throw new InvalidOperationException(...)
       };
   }
   ```

3. **Update Configuration:**
   ```json
   {
     "VaultSettings": {
       "Provider": "Aws",
       "AwsRegion": "us-east-1"
     }
   }
   ```

### Adding Authentication/Authorization

```csharp
// In Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* configure */ });

builder.Services.AddAuthorization();

// In controller
[Authorize]
[ApiController]
public class SecretsController : ControllerBase
{
    // Endpoints now require authentication
}
```

## Security Considerations

### ? Secret Transmission
- Always use HTTPS in production
- Consider encrypting response payloads for sensitive secrets
- Use short-lived access tokens

### ? Audit Logging
- Enable cloud provider audit logs (GCP Cloud Audit Logs)
- Log all secret access attempts
- Monitor for unusual patterns

### ? Least Privilege
- Service account needs only "Secret Manager Secret Accessor" role for read-only
- Use "Secret Manager Admin" only if storing secrets via API

### ? Rate Limiting
- Consider implementing rate limiting on endpoints
- Prevent abuse of batch endpoint

### ? Input Validation
- Secret IDs are validated for null/empty
- Consider adding regex validation for allowed characters

## Troubleshooting

### API Fails to Start

**Error:** "Failed to connect to vault at startup"

**Solutions:**
1. Check provider configuration in appsettings.json
2. Verify GOOGLE_APPLICATION_CREDENTIALS for GCP
3. Ensure vault file exists for DPAPI
4. Check logs for detailed error message

### Secrets Return 404

**Error:** Secret not found

**Solutions:**
1. Verify secret was stored using UI first
2. Check secret name spelling (case-sensitive)
3. Confirm provider matches (GCP vs DPAPI)
4. Check vault connection status

### Slow Performance

**Symptoms:** Requests take > 100ms

**Solutions:**
1. Verify singleton pattern is used (check Program.cs)
2. Check network connectivity to cloud provider
3. Review logs for connection attempts
4. Consider using DPAPI for local testing

## Monitoring and Observability

### Structured Logging

The API uses structured logging throughout:

```csharp
_logger.LogInformation("Retrieving secret: {SecretId}", id);
_logger.LogWarning("Secret not found: {SecretId}", id);
_logger.LogError(ex, "Error retrieving secret: {SecretId}", id);
```

### Health Checks (Future Enhancement)

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<VaultHealthCheck>("vault");
```

### Metrics (Future Enhancement)

- Secret retrieval count
- Average retrieval time
- Cache hit rate (if caching added)
- Error rate

## Best Practices Summary

? **DO:**
- Connect once at startup (singleton pattern)
- Use async/await throughout
- Enable structured logging
- Validate input parameters
- Handle errors gracefully
- Use configuration for provider selection
- Enable Swagger in development

? **DON'T:**
- Connect/disconnect per request
- Block async operations with .Wait() or .Result in request handlers
- Return raw exceptions to clients
- Hardcode provider selection
- Skip input validation
- Cache secrets in memory without expiration

## Production Deployment Checklist

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure proper logging (Application Insights, Serilog, etc.)
- [ ] Enable HTTPS with valid certificate
- [ ] Set up authentication/authorization
- [ ] Configure rate limiting
- [ ] Enable health checks
- [ ] Set up monitoring and alerts
- [ ] Review and restrict CORS policy
- [ ] Audit service account permissions
- [ ] Test failover scenarios
- [ ] Document secret naming conventions

---

**For questions or issues, refer to the main README.md or create a GitHub issue.**
