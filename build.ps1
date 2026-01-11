# Build Runner in debug mode
Write-Host "Building TrayMaster (Debug)..." -ForegroundColor Cyan

# Check if dotnet is available
try {
    $version = dotnet --version
    Write-Host ".NET SDK Version: $version" -ForegroundColor Green
} catch {
    Write-Host "Error: .NET SDK not found!" -ForegroundColor Red
    Write-Host "Run: .\install-dotnet.ps1" -ForegroundColor Yellow
    exit 1
}

# Build
dotnet build -c Debug

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful!" -ForegroundColor Green

    # Copy TrayMasterConfig.json to output
    $outputDir = "bin\Debug\net8.0-windows"
    if (Test-Path $outputDir) {
        Copy-Item TrayMasterConfig.json $outputDir -Force
        Write-Host "Config copied to $outputDir" -ForegroundColor Cyan

        # Show how to run
        Write-Host "`nTo run:" -ForegroundColor Yellow
        Write-Host "  cd $outputDir" -ForegroundColor White
        Write-Host "  .\TrayMaster.exe" -ForegroundColor White

        # Ask if user wants to run now
        Write-Host "`nRun now? (Y/N): " -NoNewline -ForegroundColor Yellow
        $response = Read-Host
        if ($response -eq 'Y' -or $response -eq 'y') {
            & "$outputDir\TrayMaster.exe"
        }
    }
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
}
