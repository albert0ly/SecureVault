# GCP Credentials Setup Script for SecureVault
# This script helps you set up Google Cloud credentials for the SecureVault application

Write-Host "=== SecureVault GCP Credentials Setup ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check if credentials directory exists
$credentialsDir = "$env:USERPROFILE\.gcp"
if (-not (Test-Path $credentialsDir)) {
    Write-Host "Creating credentials directory: $credentialsDir" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $credentialsDir -Force | Out-Null
    Write-Host "? Directory created" -ForegroundColor Green
} else {
    Write-Host "? Credentials directory exists: $credentialsDir" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Instructions to Download GCP Service Account Key ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Go to: https://console.cloud.google.com" -ForegroundColor White
Write-Host "2. Select your SecureVault project" -ForegroundColor White
Write-Host "3. Navigate to: IAM & Admin ? Service Accounts" -ForegroundColor White
Write-Host "4. Find the service account for SecureVault" -ForegroundColor White
Write-Host "5. Click the three dots (?) ? Manage keys" -ForegroundColor White
Write-Host "6. Click 'Add Key' ? 'Create new key'" -ForegroundColor White
Write-Host "7. Select 'JSON' format" -ForegroundColor White
Write-Host "8. Click 'Create' (file downloads automatically)" -ForegroundColor White
Write-Host ""
Write-Host "9. Move the downloaded JSON file to: $credentialsDir" -ForegroundColor Yellow
Write-Host "   Example: $credentialsDir\securevault-key.json" -ForegroundColor Yellow
Write-Host ""

# Step 2: Check for JSON files in common locations
Write-Host "=== Searching for JSON key files ===" -ForegroundColor Cyan
$found = $false

$searchLocations = @(
    "$env:USERPROFILE\Downloads",
    $credentialsDir,
    "$env:USERPROFILE\source\repos\SecureVault"
)

foreach ($location in $searchLocations) {
    if (Test-Path $location) {
        $jsonFiles = Get-ChildItem -Path $location -Filter "*.json" -File -ErrorAction SilentlyContinue
        if ($jsonFiles) {
            Write-Host "Found JSON files in $location" -ForegroundColor Green
            foreach ($file in $jsonFiles) {
                Write-Host "  - $($file.FullName)" -ForegroundColor Gray
            }
            $found = $true
        }
    }
}

if (-not $found) {
    Write-Host "No JSON key files found. Please download from GCP Console first." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "After downloading, run this script again." -ForegroundColor Yellow
    exit
}

Write-Host ""
Write-Host "=== Set Environment Variable ===" -ForegroundColor Cyan
Write-Host "Please enter the FULL path to your GCP service account key file:" -ForegroundColor Yellow
Write-Host "Example: $credentialsDir\securevault-key.json" -ForegroundColor Gray
Write-Host ""

$keyPath = Read-Host "Key file path"

if ([string]::IsNullOrWhiteSpace($keyPath)) {
    Write-Host "? No path provided. Exiting." -ForegroundColor Red
    exit
}

# Expand environment variables and resolve path
$keyPath = [System.Environment]::ExpandEnvironmentVariables($keyPath)

# Check if file exists
if (-not (Test-Path $keyPath)) {
    Write-Host "? File not found: $keyPath" -ForegroundColor Red
    Write-Host "Please verify the path and try again." -ForegroundColor Yellow
    exit
}

Write-Host "? File found: $keyPath" -ForegroundColor Green
Write-Host ""

# Set environment variable
Write-Host "Setting GOOGLE_APPLICATION_CREDENTIALS environment variable..." -ForegroundColor Yellow
try {
    # Set for current session
    $env:GOOGLE_APPLICATION_CREDENTIALS = $keyPath
    
    # Set permanently for user
    [System.Environment]::SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", $keyPath, [System.EnvironmentVariableTarget]::User)
    
    Write-Host "? Environment variable set successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Current value: $env:GOOGLE_APPLICATION_CREDENTIALS" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "=== IMPORTANT ===" -ForegroundColor Yellow
    Write-Host "Please RESTART Visual Studio for the changes to take effect!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "After restarting, verify the setup by running:" -ForegroundColor White
    Write-Host "  `$env:GOOGLE_APPLICATION_CREDENTIALS" -ForegroundColor Gray
    Write-Host ""
    Write-Host "? Setup complete!" -ForegroundColor Green
} catch {
    Write-Host "? Failed to set environment variable: $_" -ForegroundColor Red
}
