param(
    [string]$OutputPath = "./keys"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$toolPath = Join-Path $root ".tools"
$keyPath = Resolve-Path -LiteralPath $OutputPath -ErrorAction SilentlyContinue
if (-not $keyPath) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    $keyPath = Resolve-Path -LiteralPath $OutputPath
}

if (-not (Test-Path (Join-Path $toolPath "netsparkle-generate-appcast.exe"))) {
    dotnet tool install --tool-path $toolPath NetSparkleUpdater.Tools.AppCastGenerator --version 2.9.0
}

& (Join-Path $toolPath "netsparkle-generate-appcast") --generate-keys --force --key-path $keyPath

$publicKeyFile = Join-Path $keyPath "NetSparkle_Ed25519.pub"
$privateKeyFile = Join-Path $keyPath "NetSparkle_Ed25519.priv"

if (-not (Test-Path $publicKeyFile) -or -not (Test-Path $privateKeyFile)) {
    throw "Key files were not generated"
}

$publicKey = (Get-Content $publicKeyFile -Raw).Trim()

Write-Host "Keys generated:"
Write-Host "  Public:  $publicKeyFile"
Write-Host "  Private: $privateKeyFile"
Write-Host ""
Write-Host "Public key value (put into app settings/defaults):"
Write-Host $publicKey

