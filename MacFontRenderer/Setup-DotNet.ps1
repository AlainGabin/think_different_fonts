# Setup dotnet PATH for MacFontRenderer Development
# Run this in PowerShell as Administrator to make dotnet available globally

$dotnetPath = "C:\Program Files\dotnet"

# Check if path already exists
if ($env:PATH -like "*$dotnetPath*") {
    Write-Host "✓ dotnet is already in PATH" -ForegroundColor Green
    Write-Host "You can now use 'dotnet' from any terminal" -ForegroundColor Green
    exit
}

# Add to current session
$env:PATH = "$dotnetPath;$env:PATH"
Write-Host "✓ Added dotnet to current session PATH" -ForegroundColor Green

# Add to machine-level PATH (requires admin)
try {
    $currentPath = [System.Environment]::GetEnvironmentVariable("PATH", [System.EnvironmentVariableTarget]::Machine)
    if (-not ($currentPath -like "*$dotnetPath*")) {
        [System.Environment]::SetEnvironmentVariable("PATH", "$dotnetPath;$currentPath", [System.EnvironmentVariableTarget]::Machine)
        Write-Host "✓ Added dotnet to machine-level PATH (permanent)" -ForegroundColor Green
        Write-Host "Note: Open a new terminal to use the updated PATH" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠ Could not update machine PATH. Make sure to run as Administrator." -ForegroundColor Yellow
}

# Verify
& "$dotnetPath\dotnet.exe" --version
Write-Host "✓ dotnet is ready to use!" -ForegroundColor Green
