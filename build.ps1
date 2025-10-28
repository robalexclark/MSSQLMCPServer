#!/usr/bin/env pwsh
<#
 .SYNOPSIS
  Generates RELEASE_NOTES.md from the git commit history.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-RepoRoot {
    if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        return $PSScriptRoot
    }

    return (Get-Location).ProviderPath
}

function Get-CommitLog {
    param(
        [string]$Range
    )

    $arguments = @('--no-merges', '--pretty=format:- %s')

    if ([string]::IsNullOrWhiteSpace($Range)) {
        $arguments += @('-n', '50')
    } else {
        $arguments += $Range
    }

    $log = git log @arguments

    if ([string]::IsNullOrWhiteSpace($log)) {
        return @('- No commits found')
    }

    return $log -split "`n"
}

function Get-ReleaseHeader {
    param(
        [string]$CurrentTag
    )

    if ([string]::IsNullOrWhiteSpace($CurrentTag)) {
        return '## Unreleased'
    }

    $date = Get-Date -Format 'yyyy-MM-dd'
    return "## $CurrentTag ($date)"
}

$repoRoot = Get-RepoRoot
Push-Location $repoRoot

try {
    $tags = git tag --sort=-creatordate
    $currentTag = ''
    $previousTag = ''

    if ($tags) {
        $tagsList = $tags -split "`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        if ($tagsList.Length -gt 0) {
            $currentTag = $tagsList[0]
        }

        if ($tagsList.Length -gt 1) {
            $previousTag = $tagsList[1]
        }
    }

    $commitRange = if ($previousTag) { "$previousTag..HEAD" } else { '' }
    $releaseNotesPath = Join-Path $repoRoot 'RELEASE_NOTES.md'

    $content = @(
        '# Release Notes',
        '',
        (Get-ReleaseHeader -CurrentTag $currentTag),
        ''
    )

    $content += Get-CommitLog -Range $commitRange

    Set-Content -Path $releaseNotesPath -Value $content -Encoding UTF8
    Write-Host "Generated release notes at $releaseNotesPath"
} finally {
    Pop-Location
}
