# Prepare distribution folder with all necessary files
Write-Host "Preparing distribution folder..." -ForegroundColor Cyan

# Create dist folder if it doesn't exist
$distPath = "dist"
if (-not (Test-Path $distPath)) {
    New-Item -ItemType Directory -Path $distPath | Out-Null
}

# Copy executable
$exePath = "bin\Release\net8.0-windows\win-x64\publish\TrayMaster.exe"
if (Test-Path $exePath) {
    Copy-Item $exePath $distPath -Force
    Write-Host "✓ Copied TrayMaster.exe" -ForegroundColor Green
} else {
    Write-Host "✗ TrayMaster.exe not found. Build first with: dotnet publish -c Release" -ForegroundColor Red
    exit 1
}

# Copy configuration files
Copy-Item static\TrayMasterConfig.json.template $distPath\TrayMasterConfig.json -Force
Copy-Item static\icon.ico $distPath -Force
Write-Host "✓ Copied TrayMasterConfig.json template and icon.ico" -ForegroundColor Green

# Copy documentation
Copy-Item README.md $distPath -Force
Copy-Item examples\HTTP-REFERENCE.md $distPath -Force
Write-Host "✓ Copied documentation" -ForegroundColor Green

# Copy examples
if (Test-Path "examples") {
    Copy-Item examples\* $distPath -Force
    Write-Host "✓ Copied example handlers" -ForegroundColor Green
}

Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Distribution folder ready!" -ForegroundColor Green
Write-Host "Location: $distPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "Contents:" -ForegroundColor Yellow
Get-ChildItem $distPath | Format-Table Name, Length, LastWriteTime -AutoSize
