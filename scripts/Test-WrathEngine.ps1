$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$source = Join-Path $root 'Engine/Upstream/WrathCombo'
$runtime = Get-Content (Join-Path $root 'Engine/ReadOnlyRuntime.cs') -Raw
$provider = Get-Content (Join-Path $root 'EmbeddedRotationProvider.cs') -Raw
$prediction = Get-Content (Join-Path $root 'Engine/PredictionContext.cs') -Raw
$checks = 0
function Assert-Contract([bool]$condition, [string]$message) {
    if (!$condition) { throw $message }
    $script:checks++
}
$jobs = @('PLD','MNK','WAR','DRG','BRD','WHM','BLM','SMN','SCH','NIN','MCH','DRK','AST','SAM','RDM','BLU','GNB','DNC','RPR','SGE','VPR','PCT')
$presets = Get-Content (Join-Path $source 'Combos/CustomComboPreset.cs') -Raw
# Check real upstream evaluator classes and their annotated ST/AoE entry points,
# not just the display's list of job names. These are source contracts, not an
# in-game combat/parity test.
$definitions = [regex]::Matches($presets, '(?ms)(?<attributes>(?:\s*\[[^\r\n]+\]\s*)+)\s*(?<name>\w+)\s*=\s*\d+,')
foreach ($job in $jobs) {
    $rules = (Get-ChildItem (Join-Path $source "Combos/PvE/$job") -Filter '*.cs' | Get-Content -Raw) -join "`n"
    foreach ($aoe in @('false','true')) {
        $roots = @($definitions | Where-Object {
            $_.Groups['attributes'].Value -match "\[JobInfo\(Job\.$job(?:\)|,)" -and
            $_.Groups['attributes'].Value -match "\[AutoAction\($aoe, false\)\]" -and
            $_.Groups['attributes'].Value -match '\[(?:Simple|Advanced)DPSCombo\]'
        })
        Assert-Contract ($roots.Count -gt 0) "$job lacks an upstream DPS evaluator for AoE=$aoe"
        foreach ($entry in $roots) {
            $name = $entry.Groups['name'].Value
            Assert-Contract ($rules -match "override\s+Preset\s+Preset\s*=>\s*Preset\.$name;") "$name metadata has no actual evaluator"
            Assert-Contract ($entry.Groups['attributes'].Value -match '\[ReplaceSkill\(') "$name has no verified action entry point"
        }
    }
}
Assert-Contract ($runtime -match 'Active => true' -and $runtime -notmatch 'Active\s*\{[^}]*set') 'Read-only is an immutable assembly invariant'
Assert-Contract ($runtime -match 'combo.Suggest\(entry\)' -and $runtime -notmatch '\.TryInvoke\(') 'Selection must not be gated by Wrath enabled state or IPC action requests'
Assert-Contract ($runtime -notmatch 'new AutoRotationController|Provider.Init|new MovementHook|Module.All|RegisterCommands') 'No upstream automation may be started'
Assert-Contract ($runtime -match 'PredictionContext\.Begin\(\)' -and $runtime -match 'timeline\.Advance\(prior\)') 'Forecast must advance an isolated timeline before every future Wrath decision'
Assert-Contract ($provider -match 'runtime\.Forecast' -and $provider -notmatch 'SingleTargetCombo|AoeCombo|Array\.IndexOf') 'Preview cannot fall back to a hard-coded basic combo list'
Assert-Contract ($prediction -match 'ComboAction' -and $prediction -match 'CooldownRemaining' -and $prediction -match 'RemainingGcd' -and $prediction -match 'Weaves') 'Prediction timeline must project combo, cooldown and weave state'
Assert-Contract ($prediction -notmatch 'UseAction\(|SetTarget|QueuedActionId\s*=') 'Prediction timeline cannot issue actions, retarget, or alter queues'
Assert-Contract ($runtime.IndexOf('P = this;') -lt $runtime.IndexOf('UIHelper = new UIHelper')) 'Private singleton must exist before UIHelper initialization'
$facade = Get-Content (Join-Path $root 'Engine/ReadOnlyActionReplacer.cs') -Raw
Assert-Contract ($facade -notmatch 'HookFrom|UseAction\(') 'Decision adapter may neither hook hotbars nor execute actions'
$watcher = Get-Content (Join-Path $source 'Data/ActionWatching.cs') -Raw
$observer = $watcher.Substring($watcher.IndexOf('private static unsafe void ObserveSentAction('))
$observer = $observer.Substring(0, $observer.IndexOf('private unsafe static void SendActionDetour('))
Assert-Contract (([regex]::Matches($observer, 'SendActionHook!\.Original\(')).Count -eq 1) 'Outgoing observer must forward exactly once'
Assert-Contract ($observer.IndexOf('SendActionHook!.Original(') -lt $observer.IndexOf('try')) 'Original send must occur before fallible managed observation'
Assert-Contract ($observer -notmatch 'UseAction\(|SetTarget|\.Target\s*=|\.QueuedActionId\s*=') 'Observer cannot issue actions, retarget, or alter queues'
$safety = @{
    'Core/ConfigurationHelper.cs' = 'if \(ReadOnlyRuntime.Active\)\s*return;'
    'Combos/PvE/ALL/Items.cs' = 'if \(ReadOnlyRuntime.Active\) return;'
    'CustomCombo/Functions/Status.cs' = 'hasActionPenalty && !ReadOnlyRuntime.Active'
    'Services/BlueMageService.cs' = 'if \(ReadOnlyRuntime.Active\) return;'
    'AutoRotation/AutoRotationController.cs' = 'if \(ReadOnlyRuntime.Active\) return;'
    'CustomCombo/StancePartner.cs' = 'if \(ReadOnlyRuntime.Active\) return;'
}
foreach ($file in $safety.Keys) {
    Assert-Contract ((Get-Content (Join-Path $source $file) -Raw) -match $safety[$file]) "Read-only guard missing from $file"
}
Write-Host "Wrath engine source/safety contracts passed: $checks checks across all 22 jobs. Live game parity is a separate test."
