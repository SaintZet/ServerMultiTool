param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$TargetBranch = "main",
    [string]$Title,
    [string]$Notes = "",
    [switch]$Draft,
    [switch]$Prerelease
)

$ErrorActionPreference = "Stop"

if (-not $Title) {
    $Title = $Version
}

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) is not installed. Install: https://cli.github.com/"
}

$tag = if ($Version.StartsWith("v")) { $Version } else { "v$Version" }

# Create and push tag if it doesn't exist
$existingTag = git tag --list $tag
if ([string]::IsNullOrWhiteSpace($existingTag)) {
    git tag $tag
    git push origin $tag
}

$releaseArgs = @("release", "create", $tag, "--target", $TargetBranch, "--title", $Title)

if (-not [string]::IsNullOrWhiteSpace($Notes)) {
    $releaseArgs += @("--notes", $Notes)
} else {
    $releaseArgs += "--generate-notes"
}

if ($Draft) { $releaseArgs += "--draft" }
if ($Prerelease) { $releaseArgs += "--prerelease" }

& gh @releaseArgs

Write-Host "Release created: $tag"
Write-Host "GitHub Actions workflow 'release-auto-update' will publish binaries and update appcast."

