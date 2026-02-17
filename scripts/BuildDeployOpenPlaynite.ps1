param(
    [string]$Configuration = "Debug",
    [switch]$NoLaunch
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $projectRoot "LocalAchievements.csproj"
$outputDir = Join-Path $projectRoot ("bin\" + $Configuration)
$extensionDir = Join-Path $env:APPDATA "Playnite\Extensions\LocalAchievements"

Write-Host "Building project ($Configuration)..."
dotnet build $projectFile -c $Configuration -p:PostBuildEvent=
if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE."
}

Write-Host "Copying build output to Playnite extension folder..."
New-Item -ItemType Directory -Path $extensionDir -Force | Out-Null
try {
    Copy-Item (Join-Path $outputDir "*") $extensionDir -Recurse -Force
}
catch {
    Write-Warning "Some files could not be copied (likely locked by a running Playnite instance)."
}

if ($NoLaunch) {
    Write-Host "NoLaunch enabled. Skipping Playnite launch."
    exit 0
}

$playniteCandidates = @(
    (Join-Path $env:LOCALAPPDATA "Playnite\Playnite.DesktopApp.exe"),
    (Join-Path $env:ProgramFiles "Playnite\Playnite.DesktopApp.exe"),
    (Join-Path ${env:ProgramFiles(x86)} "Playnite\Playnite.DesktopApp.exe")
)

$playniteExe = $playniteCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $playniteExe) {
    Write-Warning "Playnite executable not found in common install locations."
    exit 0
}

$playniteRunning = Get-Process -Name "Playnite.DesktopApp" -ErrorAction SilentlyContinue
if ($playniteRunning) {
    Write-Host "Playnite is already running. Skipping launch."
}
else {
    Write-Host "Starting Playnite..."
    Start-Process -FilePath $playniteExe
}
