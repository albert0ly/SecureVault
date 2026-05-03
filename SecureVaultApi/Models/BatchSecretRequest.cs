namespace SecureVaultApi.Models
{
    /// <summary>
    /// Request model for batch secret retrieval.
    /// </summary>
    public class BatchSecretRequest
    {
        /// <summary>
        /// Array of secret identifiers to retrieve.
        /// </summary>
        public string[] SecretIds { get; set; } = Array.Empty<string>();
    }
}
