namespace SecureVaultApi.Models
{
    /// <summary>
    /// Response model for secret retrieval.
    /// </summary>
    public class SecretResponse
    {
        /// <summary>
        /// The secret identifier/name.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The secret value (null if not found).
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Indicates if the secret was found.
        /// </summary>
        public bool Found { get; set; }
    }
}
