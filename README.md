# VieriRotationHelper

VieriRotationHelper is a manual-execution rotation suggestion overlay for
Final Fantasy XIV. It displays separate single-target and AoE bars for every
standard combat job and never executes actions.

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

Both bars open automatically and are visible out of combat by default so they
can be positioned immediately. Visibility can then be restricted in settings.

## Upstream maintenance

Wrath provenance is pinned in `upstream/wrath.lock.json`. Run
`scripts/Audit-WrathUpdate.ps1` with a newer Wrath checkout to produce the
all-job compatibility report before integrating an update. See
`docs/WRATH_INTEGRATION.md`.
