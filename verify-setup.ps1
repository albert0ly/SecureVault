# SecureVault Environment Verification Script
# Run this to check if your GCP credentials are properly configured

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " SecureVault Environment Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# Check 1: Environment Variable
Write-Host "[1/5] Checking GOOGLE_APPLICATION_CREDENTIALS environment variable..." -ForegroundColor Yellow
$envVar = [System.Environment]::GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "User")

if ([string]::IsNullOrEmpty($envVar)) {
    Write-Host "  ? FAIL: Environment variable is NOT set" -ForegroundColor Red
    Write-Host "    Action: Run setup-gcp-credentials.ps1 or use:" -ForegroundColor Yellow
    Write-Host "    setx GOOGLE_APPLICATION_CREDENTIALS `"C:\Users\albertly\.gcp\securevault-key.json`"" -ForegroundColor Gray
    $allGood = $false
} else {
    Write-Host "  ? PASS: Environment variable is set" -ForegroundColor Green
    Write-Host "    Value: $envVar" -ForegroundColor Gray
}
Write-Host ""

# Check 2: Current Session Value
Write-Host "[2/5] Checking current PowerShell session..." -ForegroundColor Yellow
$sessionVar = $env:GOOGLE_APPLICATION_CREDENTIALS

if ([string]::IsNullOrEmpty($sessionVar)) {
    Write-Host "  ? WARNING: Not set in current session" -ForegroundColor Yellow
    Write-Host "    This is normal if you just set it with setx" -ForegroundColor Gray
    Write-Host "    Restart PowerShell or Visual Studio to apply" -ForegroundColor Gray
    
    if (![string]::IsNullOrEmpty($envVar)) {
        Write-Host "    Using user-level value for verification..." -ForegroundColor Cyan
        $sessionVar = $envVar
    }
} else {
    Write-Host "  ? PASS: Set in current session" -ForegroundColor Green
    Write-Host "    Value: $sessionVar" -ForegroundColor Gray
}
Write-Host ""

# Check 3: File Exists
Write-Host "[3/5] Checking if key file exists..." -ForegroundColor Yellow
if ([string]::IsNullOrEmpty($sessionVar)) {
    Write-Host "  ? SKIP: Cannot check (no path set)" -ForegroundColor Gray
} else {
    if (Test-Path $sessionVar) {
        Write-Host "  ? PASS: File exists" -ForegroundColor Green
        Write-Host "    Path: $sessionVar" -ForegroundColor Gray
        
        # Get file info
        $fileInfo = Get-Item $sessionVar
        Write-Host "    Size: $($fileInfo.Length) bytes" -ForegroundColor Gray
        Write-Host "    Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
    } else {
        Write-Host "  ? FAIL: File does NOT exist" -ForegroundColor Red
        Write-Host "    Path: $sessionVar" -ForegroundColor Gray
        Write-Host "    Action: Download the key from GCP Console" -ForegroundColor Yellow
        $allGood = $false
    }
}
Write-Host ""

# Check 4: File Content Validation
Write-Host "[4/5] Validating key file content..." -ForegroundColor Yellow
if ([string]::IsNullOrEmpty($sessionVar)) {
    Write-Host "  ? SKIP: Cannot check (no path set)" -ForegroundColor Gray
} elseif (!(Test-Path $sessionVar)) {
    Write-Host "  ? SKIP: File doesn't exist" -ForegroundColor Gray
} else {
    try {
        $keyContent = Get-Content $sessionVar -Raw | ConvertFrom-Json
        
        if ($keyContent.type -eq "service_account") {
            Write-Host "  ? PASS: Valid service account key" -ForegroundColor Green
            Write-Host "    Type: $($keyContent.type)" -ForegroundColor Gray
            Write-Host "    Project ID: $($keyContent.project_id)" -ForegroundColor Gray
            Write-Host "    Client Email: $($keyContent.client_email)" -ForegroundColor Gray
        } else {
            Write-Host "  ? FAIL: Not a service account key" -ForegroundColor Red
            Write-Host "    Type: $($keyContent.type)" -ForegroundColor Gray
            $allGood = $false
        }
    } catch {
        Write-Host "  ? FAIL: Invalid JSON file" -ForegroundColor Red
        Write-Host "    Error: $_" -ForegroundColor Gray
        $allGood = $false
    }
}
Write-Host ""

# Check 5: .NET SDK
Write-Host "[5/5] Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? PASS: .NET SDK installed" -ForegroundColor Green
        Write-Host "    Version: $dotnetVersion" -ForegroundColor Gray
    } else {
        Write-Host "  ? FAIL: .NET SDK not found" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "  ? FAIL: .NET SDK not found" -ForegroundColor Red
    Write-Host "    Error: $_" -ForegroundColor Gray
    $allGood = $false
}
Write-Host ""

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($allGood) {
    Write-Host "? ALL CHECKS PASSED!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Your environment is properly configured." -ForegroundColor Green
    Write-Host "You can now run the SecureVault applications:" -ForegroundColor White
    Write-Host ""
    Write-Host "  WPF UI:  dotnet run --project SecureVaultUI" -ForegroundColor Cyan
    Write-Host "  Web API: dotnet run --project SecureVaultApi" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host "? SOME CHECKS FAILED" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please fix the issues above before running the application." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Quick fix:" -ForegroundColor White
    Write-Host "  1. Run: .\setup-gcp-credentials.ps1" -ForegroundColor Cyan
    Write-Host "  2. Follow the prompts to download and set up your GCP key" -ForegroundColor Cyan
    Write-Host "  3. Restart PowerShell or Visual Studio" -ForegroundColor Cyan
    Write-Host "  4. Run this script again: .\verify-setup.ps1" -ForegroundColor Cyan
    Write-Host ""
}

# Additional help
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Need Help?" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Documentation:" -ForegroundColor White
Write-Host "  • CLONE_SETUP_GUIDE.md - Complete setup guide for new computers" -ForegroundColor Gray
Write-Host "  • GCP_SETUP.md - GCP configuration details" -ForegroundColor Gray
Write-Host "  • GETTING_STARTED.md - Quick start guide" -ForegroundColor Gray
Write-Host "  • README.md - Full project documentation" -ForegroundColor Gray
Write-Host ""
Write-Host "Scripts:" -ForegroundColor White
Write-Host "  • .\setup-gcp-credentials.ps1 - Automated setup" -ForegroundColor Gray
Write-Host "  • .\verify-setup.ps1 - This script" -ForegroundColor Gray
Write-Host ""
