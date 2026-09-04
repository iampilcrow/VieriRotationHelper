# VieriRotationHelper

VieriRotationHelper is a manual-execution rotation suggestion overlay for
Final Fantasy XIV. It displays separate single-target, AoE, and live dynamic
bars for every standard combat job and never executes actions.

The dynamic bar changes between single-target and AoE suggestions from the
number of valid hostile targets within the active AoE's real radius. Action
icons use Hilda's game-frame and recast textures, side-by-side 65/50-pixel
layout, outlined keyboard hotkeys in its Miedinger font, green positional
symbols, weave symbols, charge counts, enemy counts, and range dimming.
No spell names, mode banners, colored lead borders, or separate GCD strips
appear on the bars. Open settings and hover an icon for names/diagnostics.

## Provider model

- When Wrath Combo is loaded, the lead icon is read from the exact action
  replacement entry point Wrath currently exposes for the active job and mode.
- When Wrath Combo is disabled, the embedded Vieri provider continues to
  produce suggestions.
- The smaller remaining icons are a simulated forecast, not additional live
  Wrath outputs. This display update does not change the provider logic.
- Debug mode exposes live source and parity state in settings-open tooltips.

## Commands

- `/vrh` opens settings.
- `/vrh toggle` enables or disables the suggestion bars.

All three bars open automatically in separate default positions and are visible
out of combat by default so they can be positioned immediately. Each fixed bar
and the dynamic bar can be toggled separately.
Keyboard hotkeys are read from the current character's hotbar labels. A live
Wrath lead resolves to its real replacement button. Unbound or unknown actions
do not receive invented hotkeys. Unlock bars to drag them into place.

## Upstream maintenance

Wrath provenance is pinned in `upstream/wrath.lock.json`. Run
`scripts/Audit-WrathUpdate.ps1` with a newer Wrath checkout to produce the
all-job compatibility report before integrating an update. See
`docs/WRATH_INTEGRATION.md`.

The authorized Hilda display reference is pinned separately in
`upstream/hilda.lock.json`; see `docs/HILDA_REFERENCE.md`.
