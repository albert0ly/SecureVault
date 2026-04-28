using System;
using System.Threading.Tasks;

namespace SecureVaultInterface
{
    /// <summary>
    /// Interface for key vault operations.
    /// Implementations can support GCP Secret Manager, AWS Secrets Manager, Azure Key Vault, DPAPI, etc.
    /// </summary>
    public interface IKeyVault
    {
        /// <summary>
        /// Connects to the key vault service.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ConnectAsync();

        /// <summary>
        /// Disconnects from the key vault service.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DisconnectAsync();

        /// <summary>
        /// Stores a secret in the vault.
        /// </summary>
        /// <param name="secretName">The name/identifier of the secret</param>
        /// <param name="secretValue">The secret value to store</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StoreAsync(string secretName, string secretValue);

        /// <summary>
        /// Retrieves a secret from the vault.
        /// </summary>
        /// <param name="secretName">The name/identifier of the secret to retrieve</param>
        /// <returns>The secret value, or null if not found.</returns>
        Task<string?> RetrieveAsync(string secretName);

        /// <summary>
        /// Checks if the vault is currently connected.
        /// </summary>
        /// <returns>True if connected, false otherwise.</returns>
        bool IsConnected { get; }
    }
}
