using SecureVaultInterface;
using GcpSecretManagerVault;
using DpapiVault;
using System;
using System.Windows;
using System.Windows.Controls;
using GcpVault = GcpSecretManagerVault.GcpSecretManagerVault;

namespace SecureVaultUI
{
    public partial class MainWindow : Window
    {
        private IKeyVault? _vault;
        private bool _isGcpMode = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void VaultTypeChanged(object sender, RoutedEventArgs e)
        {
            if (DpapiRadioButton == null || GcpRadioButton == null)
                return;

            _isGcpMode = GcpRadioButton.IsChecked == true;

            // Toggle visibility of configuration panels
            if (GcpConfigPanel != null)
                GcpConfigPanel.Visibility = _isGcpMode ? Visibility.Visible : Visibility.Collapsed;

            if (DpapiConnectButton != null)
                DpapiConnectButton.Visibility = _isGcpMode ? Visibility.Collapsed : Visibility.Visible;

            // Disconnect current vault if any
            if (_vault != null && _vault.IsConnected)
            {
                _ = _vault.DisconnectAsync();
                UpdateConnectionStatus(false);
            }

            _vault = null;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DisableUI();
                ShowMessage("Connecting...", false);

                if (_isGcpMode)
                {
                    // GCP Mode
                    string projectId = ProjectIdInput.Text?.Trim();

                    if (string.IsNullOrEmpty(projectId))
                    {
                        ShowError("Please enter a GCP Project ID");
                        return;
                    }

                    _vault = new GcpVault(projectId);
                    await _vault.ConnectAsync();
                    ShowMessage("Successfully connected to GCP Secret Manager", true);
                }
                else
                {
                    // DPAPI Mode
                    _vault = new DpapiVault.DpapiVault();
                    await _vault.ConnectAsync();
                    ShowMessage("Successfully connected to DPAPI Vault", true);
                }

                UpdateConnectionStatus(true);
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
                ShowError("Not connected to vault. Please connect first.");
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
                ShowError("Not connected to vault. Please connect first.");
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
            
            if (_isGcpMode)
            {
                ProjectIdInput.IsEnabled = !isConnected;
            }
            
            DpapiRadioButton.IsEnabled = !isConnected;
            GcpRadioButton.IsEnabled = !isConnected;
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
            if (_isGcpMode)
                ProjectIdInput.IsEnabled = false;
            
            StoreSecretNameInput.IsEnabled = false;
            StoreSecretValueInput.IsEnabled = false;
            RetrieveSecretNameInput.IsEnabled = false;
            DpapiRadioButton.IsEnabled = false;
            GcpRadioButton.IsEnabled = false;
        }

        private void EnableUI()
        {
            if (_vault == null || !_vault.IsConnected)
            {
                if (_isGcpMode)
                    ProjectIdInput.IsEnabled = true;
                
                DpapiRadioButton.IsEnabled = true;
                GcpRadioButton.IsEnabled = true;
            }
            
            StoreSecretNameInput.IsEnabled = true;
            StoreSecretValueInput.IsEnabled = true;
            RetrieveSecretNameInput.IsEnabled = true;
        }
    }
}
