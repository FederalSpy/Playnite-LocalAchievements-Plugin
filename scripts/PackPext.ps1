param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$sourceDir = Join-Path $projectRoot ("bin\" + $Configuration)
$distDir = Join-Path $projectRoot "dist"
$toolbox = Join-Path $env:LOCALAPPDATA "Playnite\Toolbox.exe"

if (-not (Test-Path $toolbox)) {
    throw "Toolbox.exe not found at: $toolbox"
}
if (-not (Test-Path $sourceDir)) {
    throw "Source directory not found: $sourceDir"
}

$extYaml = Join-Path $sourceDir "extension.yaml"
if (-not (Test-Path $extYaml)) {
    throw "extension.yaml not found in $sourceDir"
}

$idLine = Get-Content $extYaml | Where-Object { $_ -match '^Id:\s*' } | Select-Object -First 1
$nameLine = Get-Content $extYaml | Where-Object { $_ -match '^Name:\s*' } | Select-Object -First 1
$versionLine = Get-Content $extYaml | Where-Object { $_ -match '^Version:\s*' } | Select-Object -First 1

if (-not $idLine -or -not $nameLine -or -not $versionLine) {
    throw "Could not read Id/Name/Version from extension.yaml"
}

$id = ($idLine -split ':', 2)[1].Trim()
$nameRaw = ($nameLine -split ':', 2)[1].Trim()
$versionRaw = ($versionLine -split ':', 2)[1].Trim()
$versionToolbox = $versionRaw -replace '\.', '_'
$safeName = ($nameRaw -replace '[^A-Za-z0-9]+', '-').Trim('-')
if ([string]::IsNullOrWhiteSpace($safeName)) {
    $safeName = $id
}

New-Item -ItemType Directory -Force -Path $distDir | Out-Null

# Generate valid package with official toolbox format
& $toolbox pack "$sourceDir" "$distDir"

$toolboxOut = Join-Path $distDir ("{0}_{1}.pext" -f $id, $versionToolbox)
if (-not (Test-Path $toolboxOut)) {
    throw "Toolbox output not found: $toolboxOut"
}

$friendlyOut = Join-Path $distDir ("{0}-{1}.pext" -f $safeName, $versionRaw)
if (Test-Path $friendlyOut) {
    Remove-Item $friendlyOut -Force
}

Copy-Item $toolboxOut $friendlyOut -Force
Write-Host "Valid toolbox package:" $toolboxOut
Write-Host "Friendly copy:" $friendlyOut
