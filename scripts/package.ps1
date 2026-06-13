param(
    [string]$Version = "1.0.0",
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$artifacts = Join-Path $repoRoot "artifacts"
$publishRoot = Join-Path $artifacts "publish"
$appPublish = Join-Path $publishRoot "app"
$servicePublish = Join-Path $publishRoot "service"
$dist = Join-Path $repoRoot "dist"
$portableRoot = Join-Path $artifacts "portable"

$dotnet = Get-Command "dotnet" -ErrorAction SilentlyContinue
$dotnetPath = $dotnet?.Source
if (-not $dotnet) {
    $candidateDotnet = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $candidateDotnet) {
        $dotnetPath = (Get-Item $candidateDotnet).FullName
    }
}

if (-not $dotnetPath) {
    throw ".NET 8 SDK was not found. Install it from https://dotnet.microsoft.com/download/dotnet/8.0 or add dotnet.exe to PATH."
}

Remove-Item -LiteralPath $publishRoot, $portableRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $appPublish, $servicePublish, $dist -Force | Out-Null

& $dotnetPath publish (Join-Path $repoRoot "MacFontRenderer\MacFontRenderer.csproj") `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -p:Version=$Version `
    -o $appPublish

& $dotnetPath publish (Join-Path $repoRoot "MacFontRenderer.Service\MacFontRenderer.Service.csproj") `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -p:Version=$Version `
    -o $servicePublish

$zipPath = Join-Path $dist "MacFontRenderer-portable-$Version-win-x64.zip"
Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path (Join-Path $portableRoot "App"), (Join-Path $portableRoot "Service") -Force | Out-Null
Copy-Item -Path (Join-Path $appPublish "*") -Destination (Join-Path $portableRoot "App") -Recurse -Force
Copy-Item -Path (Join-Path $servicePublish "*") -Destination (Join-Path $portableRoot "Service") -Recurse -Force
Copy-Item -Path (Join-Path $repoRoot "fonts") -Destination $portableRoot -Recurse -Force
Compress-Archive -Path (Join-Path $portableRoot "*") -DestinationPath $zipPath -Force

if (-not $SkipInstaller) {
    $iscc = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    $isccPath = $iscc?.Source
    if (-not $iscc) {
        $candidatePaths = @(
            "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
            "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
            "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
        )

        foreach ($candidate in $candidatePaths) {
            if ($candidate -and (Test-Path $candidate)) {
                $isccPath = (Get-Item $candidate).FullName
                break
            }
        }
    }

    if (-not $isccPath) {
        Write-Warning "Inno Setup 6 was not found. Portable ZIP created, but installer EXE was not built."
        Write-Warning "Install Inno Setup 6, then rerun: .\scripts\package.ps1 -Version $Version"
        exit 0
    }

    $env:MACFONTRENDERER_VERSION = $Version
    & $isccPath (Join-Path $repoRoot "installer\MacFontRenderer.iss")
}

Write-Host "Package output:"
Get-ChildItem -Path $dist -Recurse -File | Select-Object FullName, Length
