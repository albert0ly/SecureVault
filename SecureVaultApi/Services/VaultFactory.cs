using SecureVaultInterface;
using GcpSecretManagerVault;
using DpapiVault;

namespace SecureVaultApi.Services
{
    /// <summary>
    /// Factory for creating IKeyVault instances based on configuration.
    /// </summary>
    public static class VaultFactory
    {
        /// <summary>
        /// Creates a vault instance based on the configuration provider setting.
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        /// <returns>IKeyVault instance</returns>
        /// <exception cref="InvalidOperationException">Thrown when provider is not supported</exception>
        public static IKeyVault CreateVault(IConfiguration configuration)
        {
            var provider = configuration["VaultSettings:Provider"]?.ToLowerInvariant();
            
            return provider switch
            {
                "gcp" => CreateGcpVault(configuration),
                "dpapi" => CreateDpapiVault(configuration),
                _ => throw new InvalidOperationException(
                    $"Unsupported vault provider: '{provider}'. Supported providers: 'Gcp', 'Dpapi'")
            };
        }

        private static IKeyVault CreateGcpVault(IConfiguration configuration)
        {
            var projectId = configuration["VaultSettings:GcpProjectId"];
            
            if (string.IsNullOrWhiteSpace(projectId))
            {
                throw new InvalidOperationException(
                    "GcpProjectId must be configured in VaultSettings when using GCP provider.");
            }
            
            return new GcpSecretManagerVault.GcpSecretManagerVault(projectId);
        }

        private static IKeyVault CreateDpapiVault(IConfiguration configuration)
        {
            var vaultPath = configuration["VaultSettings:DpapiVaultPath"];
            
            // If vaultPath is null or empty, DpapiVault will use default location
            return new DpapiVault.DpapiVault(
                string.IsNullOrWhiteSpace(vaultPath) ? null : vaultPath);
        }
    }
}
