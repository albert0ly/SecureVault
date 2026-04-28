using SecureVaultInterface;
using GcpSecretManagerVault;
using System;
using System.Windows;
using GcpVault = GcpSecretManagerVault.GcpSecretManagerVault;

namespace SecureVaultUI
{
    public partial class MainWindow : Window
    {
        private IKeyVault? _vault;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string projectId = ProjectIdInput.Text?.Trim();
            
            if (string.IsNullOrEmpty(projectId))
            {
                ShowError("Please enter a GCP Project ID");
                return;
            }

            try
            {
                DisableUI();
                ShowMessage("Connecting to GCP Secret Manager...", false);

                _vault = new GcpVault(projectId);
                await _vault.ConnectAsync();

                UpdateConnectionStatus(true);
                ShowMessage("Successfully connected to GCP Secret Manager", true);
            }
            catch (Exception ex)
            {
                UpdateConnectionStatus(false);
                ShowError($"Connection failed: {ex.Message}");
            }
            finally
            {
                EnableUI();
            }
        }

        private async void StoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vault == null || !_vault.IsConnected)
            {
                ShowError("Not connected to Secret Manager. Please connect first.");
                return;
            }

            string secretName = StoreSecretNameInput.Text?.Trim();
            string secretValue = StoreSecretValueInput.Text?.Trim();

            if (string.IsNullOrEmpty(secretName))
            {
                ShowError("Please enter a secret name");
                return;
            }

            if (string.IsNullOrEmpty(secretValue))
            {
                ShowError("Please enter a secret value");
                return;
            }

            try
            {
                DisableUI();
                ShowMessage($"Storing secret '{secretName}'...", false);

                await _vault.StoreAsync(secretName, secretValue);

                ShowMessage($"Secret '{secretName}' stored successfully!", true);
                StoreSecretNameInput.Clear();
                StoreSecretValueInput.Clear();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to store secret: {ex.Message}");
            }
            finally
            {
                EnableUI();
            }
        }

        private async void RetrieveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vault == null || !_vault.IsConnected)
            {
                ShowError("Not connected to Secret Manager. Please connect first.");
                return;
            }

            string secretName = RetrieveSecretNameInput.Text?.Trim();

            if (string.IsNullOrEmpty(secretName))
            {
                ShowError("Please enter a secret name to retrieve");
                return;
            }

            try
            {
                DisableUI();
                ShowMessage($"Retrieving secret '{secretName}'...", false);

                string? secretValue = await _vault.RetrieveAsync(secretName);

                if (secretValue == null)
                {
                    ShowError($"Secret '{secretName}' not found in vault.");
                    RetrievedSecretOutput.Text = "";
                }
                else
                {
                    RetrievedSecretOutput.Text = secretValue;
                    ShowMessage($"Secret '{secretName}' retrieved successfully!", true);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to retrieve secret: {ex.Message}");
                RetrievedSecretOutput.Text = "";
            }
            finally
            {
                EnableUI();
            }
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            StatusText.Text = isConnected ? "Connected" : "Disconnected";
            StatusIndicator.Fill = isConnected ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            ProjectIdInput.IsEnabled = !isConnected;
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            StatusMessageText.Text = message;
            StatusMessageText.Foreground = isSuccess ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.DarkOrange;
        }

        private void ShowError(string message)
        {
            StatusMessageText.Text = $"Error: {message}";
            StatusMessageText.Foreground = System.Windows.Media.Brushes.Red;
        }

        private void DisableUI()
        {
            ProjectIdInput.IsEnabled = false;
            StoreSecretNameInput.IsEnabled = false;
            StoreSecretValueInput.IsEnabled = false;
            RetrieveSecretNameInput.IsEnabled = false;
        }

        private void EnableUI()
        {
            ProjectIdInput.IsEnabled = _vault == null || !_vault.IsConnected;
            StoreSecretNameInput.IsEnabled = true;
            StoreSecretValueInput.IsEnabled = true;
            RetrieveSecretNameInput.IsEnabled = true;
        }
    }
}
