# Recreate Grok discovery junctions so slash skills and rules load from AI-AGENTS.
# Run from the repository root (folder that contains AI-AGENTS/ and training-repo/).
# Safe to re-run.

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root

if (-not (Test-Path "AI-AGENTS\rules") -or -not (Test-Path "AI-AGENTS\skills")) {
    throw "AI-AGENTS/rules or AI-AGENTS/skills not found. Run this from a full clone of the training repo."
}

New-Item -ItemType Directory -Force -Path ".grok" | Out-Null

foreach ($name in @("rules", "skills")) {
    $link = Join-Path ".grok" $name
    $target = (Resolve-Path (Join-Path "AI-AGENTS" $name)).Path
    if (Test-Path $link) {
        $item = Get-Item $link -Force
        if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            cmd /c "rmdir `"$link`""
        }
        else {
            Remove-Item $link -Recurse -Force
        }
    }
    cmd /c "mklink /J `"$link`" `"$target`""
    if ($LASTEXITCODE -ne 0) { throw "Failed to create junction for $name" }
}

Write-Host "OK: .grok/rules and .grok/skills -> AI-AGENTS/*"
Get-ChildItem ".grok\skills" | ForEach-Object { Write-Host "  skill: $($_.Name)" }
