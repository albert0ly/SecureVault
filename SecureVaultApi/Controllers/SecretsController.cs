using Microsoft.AspNetCore.Mvc;
using SecureVaultInterface;
using SecureVaultApi.Models;

namespace SecureVaultApi.Controllers
{
    /// <summary>
    /// API controller for secret management operations.
    /// Demonstrates best practices for using IKeyVault in a REST API:
    /// - Singleton vault connection (initialized at startup)
    /// - Async operations
    /// - Proper error handling
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SecretsController : ControllerBase
    {
        private readonly IKeyVault _vault;
        private readonly ILogger<SecretsController> _logger;

        public SecretsController(IKeyVault vault, ILogger<SecretsController> logger)
        {
            _vault = vault;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a single secret by its identifier.
        /// </summary>
        /// <param name="id">The secret identifier/name</param>
        /// <returns>Secret value or 404 if not found</returns>
        /// <response code="200">Secret found and returned</response>
        /// <response code="404">Secret not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SecretResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SecretResponse>> GetSecret(string id)
        {
            try
            {
                _logger.LogInformation("Retrieving secret: {SecretId}", id);

                var secretValue = await _vault.RetrieveAsync(id);

                if (secretValue == null)
                {
                    _logger.LogWarning("Secret not found: {SecretId}", id);
                    return NotFound(new SecretResponse
                    {
                        Id = id,
                        Value = null,
                        Found = false
                    });
                }

                return Ok(new SecretResponse
                {
                    Id = id,
                    Value = secretValue,
                    Found = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving secret: {SecretId}", id);
                return StatusCode(500, new { error = "Failed to retrieve secret", message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves multiple secrets in a single request.
        /// </summary>
        /// <param name="request">Array of secret identifiers</param>
        /// <returns>Array of secret key-value pairs</returns>
        /// <response code="200">Secrets retrieved (includes both found and not found)</response>
        /// <response code="400">Invalid request (empty array)</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("batch")]
        [ProducesResponseType(typeof(SecretResponse[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SecretResponse[]>> GetSecretsBatch([FromBody] BatchSecretRequest request)
        {
            if (request?.SecretIds == null || request.SecretIds.Length == 0)
            {
                return BadRequest(new { error = "SecretIds array cannot be empty" });
            }

            try
            {
                _logger.LogInformation("Retrieving batch of {Count} secrets", request.SecretIds.Length);

                var results = new List<SecretResponse>();

                // Retrieve secrets concurrently for better performance
                var tasks = request.SecretIds.Select(async id =>
                {
                    try
                    {
                        var value = await _vault.RetrieveAsync(id);
                        return new SecretResponse
                        {
                            Id = id,
                            Value = value,
                            Found = value != null
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error retrieving secret in batch: {SecretId}", id);
                        return new SecretResponse
                        {
                            Id = id,
                            Value = null,
                            Found = false
                        };
                    }
                });

                results = (await Task.WhenAll(tasks)).ToList();

                _logger.LogInformation("Batch retrieval completed: {Found}/{Total} secrets found",
                    results.Count(r => r.Found), results.Count);

                return Ok(results.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch secret retrieval");
                return StatusCode(500, new { error = "Failed to retrieve secrets", message = ex.Message });
            }
        }
    }
}
