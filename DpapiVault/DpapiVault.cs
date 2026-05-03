using SecureVaultInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DpapiVault
{
    /// <summary>
    /// Windows DPAPI implementation of IKeyVault.
    /// Stores and retrieves secrets using Windows Data Protection API (DPAPI).
    /// Secrets are encrypted for the current user and stored in a local file.
    /// </summary>
    public class DpapiVault : IKeyVault
    {
        private readonly string _vaultFilePath;
        private Dictionary<string, byte[]> _secrets;
        private bool _isConnected;

        /// <summary>
        /// Initializes a new instance of DpapiVault.
        /// </summary>
        /// <param name="vaultFilePath">Path to the vault file. If null, uses default location in user's AppData.</param>
        public DpapiVault(string? vaultFilePath = null)
        {
            if (string.IsNullOrWhiteSpace(vaultFilePath))
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string vaultDirectory = Path.Combine(appDataPath, "SecureVault");
                Directory.CreateDirectory(vaultDirectory);
                _vaultFilePath = Path.Combine(vaultDirectory, "dpapi-vault.dat");
            }
            else
            {
                _vaultFilePath = vaultFilePath!;
            }

            _secrets = new Dictionary<string, byte[]>();
            _isConnected = false;
        }

        /// <summary>
        /// Connects to the DPAPI vault (loads existing vault file if present).
        /// </summary>
        public Task ConnectAsync()
        {
            if (_isConnected)
                return Task.CompletedTask;

            try
            {
                if (File.Exists(_vaultFilePath))
                {
                    LoadVault();
                }

                _isConnected = true;
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to connect to DPAPI vault: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Disconnects from the DPAPI vault.
        /// </summary>
        public Task DisconnectAsync()
        {
            _secrets.Clear();
            _isConnected = false;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stores a secret in the DPAPI vault.
        /// </summary>
        public Task StoreAsync(string secretName, string secretValue)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected to vault. Call ConnectAsync() first.");

            if (string.IsNullOrWhiteSpace(secretName))
                throw new ArgumentException("Secret name cannot be empty.", nameof(secretName));

            if (secretValue == null)
                throw new ArgumentNullException(nameof(secretValue));

            try
            {
                // Encrypt the secret value using DPAPI
                byte[] plainBytes = Encoding.UTF8.GetBytes(secretValue);
                byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);

                // Store in memory
                _secrets[secretName] = encryptedBytes;

                // Persist to disk
                SaveVault();

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to store secret '{secretName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves a secret from the DPAPI vault.
        /// </summary>
        public Task<string?> RetrieveAsync(string secretName)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected to vault. Call ConnectAsync() first.");

            if (string.IsNullOrWhiteSpace(secretName))
                throw new ArgumentException("Secret name cannot be empty.", nameof(secretName));

            try
            {
                if (!_secrets.ContainsKey(secretName))
                    return Task.FromResult<string?>(null);

                // Decrypt the secret value using DPAPI
                byte[] encryptedBytes = _secrets[secretName];
                byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                string secretValue = Encoding.UTF8.GetString(plainBytes);

                return Task.FromResult<string?>(secretValue);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve secret '{secretName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the connection status.
        /// </summary>
        public bool IsConnected => _isConnected;

        private void SaveVault()
        {
            try
            {
                using (var fileStream = new FileStream(_vaultFilePath, FileMode.Create, FileAccess.Write))
                using (var writer = new BinaryWriter(fileStream))
                {
                    writer.Write(_secrets.Count);
                    foreach (var kvp in _secrets)
                    {
                        writer.Write(kvp.Key);
                        writer.Write(kvp.Value.Length);
                        writer.Write(kvp.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save vault to {_vaultFilePath}: {ex.Message}", ex);
            }
        }

        private void LoadVault()
        {
            try
            {
                _secrets.Clear();

                using (var fileStream = new FileStream(_vaultFilePath, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(fileStream))
                {
                    int count = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        string key = reader.ReadString();
                        int length = reader.ReadInt32();
                        byte[] value = reader.ReadBytes(length);
                        _secrets[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load vault from {_vaultFilePath}: {ex.Message}", ex);
            }
        }
    }
}
