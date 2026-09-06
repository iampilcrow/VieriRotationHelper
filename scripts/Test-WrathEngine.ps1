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
Assert-Contract ($runtime -match 'Active => PredictionContext\.Current != null') 'Prediction safety must be scoped to the forecast thread'
Assert-Contract ($runtime -match 'combo.Suggest\(entry\)' -and $runtime -notmatch '\.TryInvoke\(') 'Selection must not be gated by Wrath enabled state or IPC action requests'
Assert-Contract ($runtime -match 'new WrathCombo\(pluginInterface\)' -and $runtime -match 'Service\.ActionReplacer\.OriginalHook') 'Suite must host the full engine and retain the native resolver for suggestions'
Assert-Contract ($runtime -match 'PredictionContext\.Begin\(job\)' -and $runtime -match 'timeline\.Advance\(prior\)') 'Forecast must advance an isolated job-aware timeline before every future Wrath decision'
Assert-Contract ($provider -match 'runtime\.Forecast' -and $provider -notmatch 'SingleTargetCombo|AoeCombo|Array\.IndexOf') 'Preview cannot fall back to a hard-coded basic combo list'
Assert-Contract ($prediction -match 'ComboAction' -and $prediction -match 'CooldownRemaining' -and $prediction -match 'RemainingGcd' -and $prediction -match 'Weaves') 'Prediction timeline must project combo, cooldown and weave state'
Assert-Contract ($prediction -notmatch 'UseAction\(|SetTarget|QueuedActionId\s*=') 'Prediction timeline cannot issue actions, retarget, or alter queues'
$rdmPrediction = Get-Content (Join-Path $root 'Engine/RdmPredictionState.cs') -Raw
$rdmHelper = Get-Content (Join-Path $source 'Combos/PvE/RDM/RDM_Helper.cs') -Raw
Assert-Contract ($prediction -match 'RdmPredictionState' -and $rdmPrediction -match 'Dualcast' -and $rdmPrediction -match 'VerfireReady' -and $rdmPrediction -match 'AddMana') 'Red Mage forecasts must project Dualcast, procs, and mana instead of re-reading stale live state'
Assert-Contract ($rdmHelper -match 'PredictionContext\.Current\?\.RdmBlackMana' -and $rdmHelper -match 'PredictionContext\.Current\?\.RdmWhiteMana') 'Red Mage evaluators must consume the projected gauge during forecasts'
$comboState = Get-Content (Join-Path $root 'Engine/PredictedComboState.cs') -Raw
$traitMap = Get-Content (Join-Path $root 'Engine/TraitReplacementMap.cs') -Raw
Assert-Contract ($comboState -match 'GetSubrowExcelSheet<ReplaceAction>' -and $comboState -match 'TraitReplacementMap\.Build') 'Combo normalization must load the game-wide replacement table'
Assert-Contract ($traitMap -match 'TraitReplacement\s*=\s*3' -and $traitMap -match 'replacement\.Type == TraitReplacement' -and $traitMap -match 'while \(direct\.TryGetValue') 'Only permanent trait replacements may collapse to canonical combo actions'
Assert-Contract ($runtime -match 'HasCanonicalComboAlternative' -and $runtime -match 'IsComboContinuation' -and $runtime -match 'RestoreDisplayedComboAction') 'Forecast must preserve displayed combo states unless a canonical state produces a verified continuation'
$project = Get-Content (Join-Path $root 'Engine/VieriWrathEngine.csproj') -Raw
$wrathPlugin = Get-Content (Join-Path $source 'WrathCombo.cs') -Raw
$ipcProvider = Get-Content (Join-Path $source 'Services/IPC/Provider.cs') -Raw
$wrathConfiguration = Get-Content (Join-Path $source 'Core/Configuration.cs') -Raw
Assert-Contract ($project -notmatch 'Upstream/WrathCombo/Core/ActionReplacer.cs' -and $project -match 'WrathCombo.API') 'Full action replacement and the public IPC contract must be compiled'
Assert-Contract ($wrathPlugin -match 'Service\.AutoRotationController\s*=\s*new AutoRotationController' -and $wrathPlugin -match 'IPC\s*=\s*Provider\.Init\(\)') 'Full Auto-Rotation and IPC provider must initialize'
Assert-Contract ($ipcProvider -match '\[EzIPC\]' -and $ipcProvider -match 'IsCurrentJobAutoRotationReady') 'Wrath compatibility provider must expose automation readiness'
Assert-Contract ($wrathConfiguration -notmatch 'HideMajorChangesForVersion\s*=\s*\r?\n\s*Svc\.PluginInterface' -and $wrathConfiguration -match 'typeof\(Configuration\)\.Assembly\.GetName\(\)\.Version') 'Hosted Wrath configuration construction must not require ECommons services before engine initialization'
$switchPlugin = Get-Content (Join-Path $root 'SwitchRuntime/Plugin.cs') -Raw
Assert-Contract ($switchPlugin -match 'WrathSwitch\.BeginAutomation' -and $switchPlugin -match 'MainCommand = "/wrathswitch"') 'Embedded switch must retain its legacy IPC and command contracts'
Assert-Contract ($switchPlugin -match 'embedded \? PluginInterface\.InternalName : "WrathSwitch"') 'Embedded switch leases must use the loaded suite internal name'
foreach ($bridge in @('RotationControlBridge.cs','CombatOnlyBridge.cs','MovementSafetyBridge.cs')) {
    $bridgeSource = Get-Content (Join-Path $root "SwitchRuntime/$bridge") -Raw
    Assert-Contract ($bridgeSource -match 'leaseInternalName') "$bridge must register leases with the owning plugin identity"
}
$hotbar = Get-Content (Join-Path $root 'HotbarKeySelection.cs') -Raw
$barWindow = Get-Content (Join-Path $root 'Windows/RotationBarWindow.cs') -Raw
Assert-Contract ($hotbar -match 'forceAnchor' -and $barWindow -match 'suggestion\.UsesEntryButton') 'One-button forecasts must retain their consolidated entry hotkey'
$safety = @{
    'Core/ConfigurationHelper.cs' = 'if \(ReadOnlyRuntime.Active\)\s*return;'
    'Combos/PvE/ALL/Items.cs' = 'if \(ReadOnlyRuntime.Active\) return;'
    'CustomCombo/Functions/Status.cs' = 'hasActionPenalty && !ReadOnlyRuntime.Active'
    'Services/BlueMageService.cs' = 'if \(ReadOnlyRuntime.Active\) return;'
    'AutoRotation/AutoRotationController.cs' = 'if \(ReadOnlyRuntime.Active\) return;'
    'CustomCombo/StancePartner.cs' = 'if \(ReadOnlyRuntime.Active\) return;'
}
foreach ($file in $safety.Keys) {
    Assert-Contract ((Get-Content (Join-Path $source $file) -Raw) -match $safety[$file]) "Prediction guard missing from $file"
}
Write-Host "Unified Wrath engine/source safety contracts passed: $checks checks across all 22 jobs. Live game parity is a separate test."
