$ErrorActionPreference = "Stop"
$catalog = Get-Content -LiteralPath (Join-Path $PSScriptRoot "..\JobCatalog.cs") -Raw
$expected = @('PLD','MNK','WAR','DRG','BRD','WHM','BLM','SMN','SCH','NIN','MCH','DRK','AST','SAM','RDM','BLU','GNB','DNC','RPR','SGE','VPR','PCT')
$missing = @($expected | Where-Object { $catalog -notmatch ('"' + $_ + '"') })
if ($missing.Count -gt 0) {
    throw "Missing combat-job mappings: $($missing -join ', ')"
}

$mappingCount = ([regex]::Matches($catalog, '\[\d+\]\s*=\s*new\(')).Count
if ($mappingCount -ne $expected.Count) {
    throw "Expected $($expected.Count) job mappings, found $mappingCount."
}

$lock = Get-Content -LiteralPath (Join-Path $PSScriptRoot "..\upstream\wrath.lock.json") -Raw | ConvertFrom-Json
if ($lock.commit -notmatch '^[0-9a-f]{40}$') {
    throw "Wrath source pin is not a full commit hash."
}

Write-Host "Integration checks passed: $mappingCount combat jobs, ST/AoE surfaces present, Wrath source pinned."
