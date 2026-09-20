#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates (or regenerates) the deterministic identity of the vendored
    Software Inc / Unity reference assemblies under Trainer_v5/Trainer.Libraries.

.DESCRIPTION
    Reads Trainer_v5/Trainer.Libraries/library-manifest.json and, for every
    file it lists, confirms the file exists and its SHA-256 hash and size
    match the manifest. Fails (non-zero exit code) when a file is missing or
    its hash no longer matches, so an accidental or unreviewed change to a
    vendored assembly is caught early.

    Pass -UpdateManifest after an intentional library refresh to recompute
    the manifest from the files currently on disk instead of validating
    against it.

.EXAMPLE
    pwsh ./scripts/verify-library-compatibility.ps1

.EXAMPLE
    pwsh ./scripts/verify-library-compatibility.ps1 -UpdateManifest
#>
[CmdletBinding()]
param(
    [switch]$UpdateManifest
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$librariesDir = Join-Path $repoRoot "Trainer_v5/Trainer.Libraries"
$manifestPath = Join-Path $librariesDir "library-manifest.json"

if (-not (Test-Path $manifestPath))
{
    throw "Manifest not found at '$manifestPath'."
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json

if ($UpdateManifest)
{
    $updated = [ordered]@{
        '$comment' = $manifest.'$comment'
        files      = @()
    }

    foreach ($entry in $manifest.files)
    {
        $filePath = Join-Path $librariesDir $entry.name
        if (-not (Test-Path $filePath))
        {
            throw "Cannot update manifest: expected file '$($entry.name)' is missing from '$librariesDir'."
        }

        $hash = (Get-FileHash -Path $filePath -Algorithm SHA256).Hash.ToLowerInvariant()
        $size = (Get-Item $filePath).Length

        $updated.files += [ordered]@{
            name      = $entry.name
            sizeBytes = $size
            sha256    = $hash
        }

        Write-Host "Updated $($entry.name): $size bytes, sha256=$hash"
    }

    ($updated | ConvertTo-Json -Depth 5) | Set-Content -Path $manifestPath -Encoding utf8
    Write-Host "Manifest regenerated at '$manifestPath'."
    exit 0
}

$failures = @()

foreach ($entry in $manifest.files)
{
    $filePath = Join-Path $librariesDir $entry.name

    if (-not (Test-Path $filePath))
    {
        $failures += "MISSING: '$($entry.name)' is not present in '$librariesDir'."
        continue
    }

    $actualSize = (Get-Item $filePath).Length
    $actualHash = (Get-FileHash -Path $filePath -Algorithm SHA256).Hash.ToLowerInvariant()

    if ($actualSize -ne $entry.sizeBytes)
    {
        $failures += "SIZE MISMATCH: '$($entry.name)' is $actualSize bytes, expected $($entry.sizeBytes) bytes."
    }

    if ($actualHash -ne $entry.sha256)
    {
        $failures += "HASH MISMATCH: '$($entry.name)' sha256 is $actualHash, expected $($entry.sha256)."
    }

    if ($actualSize -eq $entry.sizeBytes -and $actualHash -eq $entry.sha256)
    {
        Write-Host "OK: $($entry.name)"
    }
}

if ($failures.Count -gt 0)
{
    Write-Host ""
    Write-Host "Library compatibility validation failed:"
    foreach ($failure in $failures)
    {
        Write-Host "  - $failure"
    }
    Write-Host ""
    Write-Host "See docs/project/library-compatibility.md for the update procedure. If this change is an intentional library refresh, re-run with -UpdateManifest and commit the regenerated manifest."
    exit 1
}

Write-Host ""
Write-Host "All vendored reference assemblies match the documented manifest."
exit 0
