using Google.Cloud.SecretManager.V1;
using Google.Protobuf;
using Grpc.Core;
using SecureVaultInterface;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GrpcChannel = Grpc.Net.Client.GrpcChannel;
using GrpcChannelOptions = Grpc.Net.Client.GrpcChannelOptions;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;

namespace GcpSecretManagerVault
{
    /// <summary>
    /// GCP Secret Manager implementation of IKeyVault.
    /// Stores and retrieves secrets from Google Cloud Secret Manager.
    /// </summary>
    public class GcpSecretManagerVault : IKeyVault
    {
        private SecretManagerServiceClient? _client;
        private string _projectId;
        private bool _isConnected;
        private HttpClient? _httpClient;

        /// <summary>
        /// Initializes a new instance of GcpSecretManagerVault.
        /// </summary>
        /// <param name="projectId">Your GCP Project ID</param>
        public GcpSecretManagerVault(string projectId)
        {
            _projectId = projectId ?? throw new ArgumentNullException(nameof(projectId));
            _isConnected = false;
        }

        /// <summary>
        /// Connects to GCP Secret Manager.
        /// </summary>
        public async Task ConnectAsync()
        {
            if (_isConnected)
                return;

            try
            {
                // Get Google credentials
                var credential = await GoogleCredential.GetApplicationDefaultAsync();
                if (credential.IsCreateScopedRequired)
                {
                    credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
                }

                // Create HttpClient with no proxy to bypass corporate proxy
                var httpHandler = new SocketsHttpHandler
                {
                    UseProxy = false,
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    EnableMultipleHttp2Connections = true
                };

                _httpClient = new HttpClient(httpHandler);
                
                var channel = GrpcChannel.ForAddress("https://secretmanager.googleapis.com", new GrpcChannelOptions
                {
                    HttpClient = _httpClient,
                    Credentials = ChannelCredentials.Create(new SslCredentials(), credential.ToCallCredentials())
                });

                var grpcClient = new SecretManagerService.SecretManagerServiceClient(channel);
                _client = new SecretManagerServiceClientImpl(grpcClient, new SecretManagerServiceSettings(), null);
                _isConnected = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to connect to GCP Secret Manager. Ensure Google Cloud credentials are properly configured and port 443 is accessible.", ex);
            }
        }

        /// <summary>
        /// Disconnects from GCP Secret Manager.
        /// </summary>
        public async Task DisconnectAsync()
        {
            _client = null;
            _httpClient?.Dispose();
            _httpClient = null;
            _isConnected = false;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Stores a secret in GCP Secret Manager.
        /// </summary>
        public async Task StoreAsync(string secretName, string secretValue)
        {
            if (!_isConnected || _client == null)
                throw new InvalidOperationException("Not connected to Secret Manager. Call ConnectAsync() first.");

            if (string.IsNullOrWhiteSpace(secretName))
                throw new ArgumentException("Secret name cannot be empty.", nameof(secretName));

            try
            {
                string projectPath = $"projects/{_projectId}";

                // Create secret if it doesn't exist
                var secret = new Secret
                {
                    Replication = new Replication
                    {
                        Automatic = new Replication.Types.Automatic()
                    }
                };

                var createSecretRequest = new CreateSecretRequest
                {
                    Parent = projectPath,
                    SecretId = secretName,
                    Secret = secret,
                };

                try
                {
                    await _client.CreateSecretAsync(createSecretRequest);
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.AlreadyExists)
                {
                    // Secret already exists, which is fine
                }

                // Add secret version with the actual value
                string secretPath = $"projects/{_projectId}/secrets/{secretName}";
                var addSecretVersionRequest = new AddSecretVersionRequest
                {
                    Parent = secretPath,
                    Payload = new SecretPayload
                    {
                        Data = ByteString.CopyFromUtf8(secretValue),
                    },
                };

                await _client.AddSecretVersionAsync(addSecretVersionRequest);
            }
            catch (RpcException ex)
            {
                throw new InvalidOperationException($"Failed to store secret '{secretName}' in GCP Secret Manager: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves a secret from GCP Secret Manager.
        /// </summary>
        public async Task<string?> RetrieveAsync(string secretName)
        {
            if (!_isConnected || _client == null)
                throw new InvalidOperationException("Not connected to Secret Manager. Call ConnectAsync() first.");

            if (string.IsNullOrWhiteSpace(secretName))
                throw new ArgumentException("Secret name cannot be empty.", nameof(secretName));

            try
            {
                string secretVersionPath = $"projects/{_projectId}/secrets/{secretName}/versions/latest";
                var response = await _client.AccessSecretVersionAsync(secretVersionPath);
                return response.Payload.Data.ToStringUtf8();
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return null; // Secret not found
            }
            catch (RpcException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve secret '{secretName}' from GCP Secret Manager: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the connection status.
        /// </summary>
        public bool IsConnected => _isConnected;
    }
}
