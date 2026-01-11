# Build Runner as single-file executable
Write-Host "Building TrayMaster (Release - Single File)..." -ForegroundColor Cyan

# Kill any running TrayMaster.exe processes
Write-Host "`nChecking for running TrayMaster.exe processes..." -ForegroundColor Yellow
$runningProcesses = Get-Process -Name "TrayMaster" -ErrorAction SilentlyContinue
if ($runningProcesses) {
    Write-Host "Stopping $($runningProcesses.Count) running TrayMaster.exe process(es)..." -ForegroundColor Yellow
    Stop-Process -Name "TrayMaster" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 500
    Write-Host "Processes stopped." -ForegroundColor Green
} else {
    Write-Host "No running TrayMaster.exe processes found." -ForegroundColor Gray
}

# Check if dotnet is available
try {
    $version = dotnet --version
    Write-Host ".NET SDK Version: $version" -ForegroundColor Green
} catch {
    Write-Host "Error: .NET SDK not found!" -ForegroundColor Red
    Write-Host "Run: .\install-dotnet.ps1" -ForegroundColor Yellow
    exit 1
}

# Clean previous builds
Write-Host "`nCleaning previous builds..." -ForegroundColor Yellow
Remove-Item -Path "bin\Release" -Recurse -Force -ErrorAction SilentlyContinue

# Build single-file executable
Write-Host "`nBuilding single-file executable..." -ForegroundColor Yellow
Write-Host "(This may take 1-2 minutes on first build)" -ForegroundColor Gray

dotnet publish -c Release

if ($LASTEXITCODE -eq 0) {
    $outputPath = "bin\Release\net8.0-windows\win-x64\publish"

    Write-Host "`nBuild successful!" -ForegroundColor Green
    Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan

    # Show file info
    $exePath = "$outputPath\TrayMaster.exe"
    if (Test-Path $exePath) {
        $size = (Get-Item $exePath).Length / 1MB
        Write-Host "Executable: $exePath" -ForegroundColor Cyan
        Write-Host ("Size: {0:N2} MB" -f $size) -ForegroundColor Yellow

        # Copy config
        Copy-Item TrayMasterConfig.json $outputPath -Force
        Write-Host "Config: $outputPath\TrayMasterConfig.json" -ForegroundColor Cyan

        # Update dist folder if it exists
        if (Test-Path "dist") {
            Copy-Item $exePath "dist\TrayMaster.exe" -Force
            Write-Host "Updated: dist\TrayMaster.exe" -ForegroundColor Green
        }
    }

    Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan

    # Ask if user wants to run
    Write-Host "`nRun now? (Y/N): " -NoNewline -ForegroundColor Yellow
    $response = Read-Host
    if ($response -eq 'Y' -or $response -eq 'y') {
        & $exePath
    }
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
}
