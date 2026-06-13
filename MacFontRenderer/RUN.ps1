# MacFontRenderer - Quick Run Script
# This script runs the application with admin privileges if needed

param(
    [switch]$AsAdmin = $false
)

$exePath = Join-Path $PSScriptRoot "bin\Release\net8.0-windows\MacFontRenderer.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "Error: Executable not found at $exePath" -ForegroundColor Red
    Write-Host "Please build the project first: dotnet build -c Release" -ForegroundColor Yellow
    exit 1
}

# Check if running as admin
$isAdmin = [Security.Principal.WindowsIdentity]::GetCurrent().Groups -contains [Security.Principal.SecurityIdentifier]"S-1-5-32-544"

if (-not $isAdmin) {
    Write-Host "MacFontRenderer requires Administrator privileges." -ForegroundColor Yellow
    Write-Host "Requesting elevation..." -ForegroundColor Cyan
    
    $arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`" -AsAdmin"
    Start-Process -FilePath "powershell.exe" -ArgumentList $arguments -Verb RunAs
    exit
}

Write-Host "Launching MacFontRenderer..." -ForegroundColor Green
& $exePath
