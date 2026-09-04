# VieriRotationHelper

VieriRotationHelper is a manual-execution rotation suggestion overlay for
Final Fantasy XIV. It displays separate single-target, AoE, and live dynamic
bars for every standard combat job and never executes actions.

The dynamic bar changes between single-target and AoE suggestions from the
number of valid hostile targets within the active AoE's real radius. Action
icons can also show Hilda-style flank/rear cues, nearby enemy counts, range
dimming, cooldown sweeps, and a GCD progress strip.

## Provider model

- When Wrath Combo is loaded, the lead icon is read from the exact action
  replacement entry point Wrath currently exposes for the active job and mode.
- When Wrath Combo is disabled, the embedded Vieri provider continues to
  produce suggestions.
- The remaining icons are a simulated forecast. They are intentionally marked
  separately from the authoritative lead action.
- Debug mode exposes live source and parity state.

## Commands

- `/vrh` opens settings.
- `/vrh toggle` enables or disables the suggestion bars.

All three bars open automatically in separate default positions and are visible
out of combat by default so they can be positioned immediately. Each fixed bar
and the dynamic bar can be toggled separately.

## Upstream maintenance

Wrath provenance is pinned in `upstream/wrath.lock.json`. Run
`scripts/Audit-WrathUpdate.ps1` with a newer Wrath checkout to produce the
all-job compatibility report before integrating an update. See
`docs/WRATH_INTEGRATION.md`.

The authorized Hilda display reference is pinned separately in
`upstream/hilda.lock.json`; see `docs/HILDA_REFERENCE.md`.
