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

- The next action is selected by the bundled, pinned Wrath PvE evaluators for
  all 22 combat jobs (and their base classes), using actual gauges, statuses,
  cooldowns, level restrictions, targets, and observed action history.
- Disabling button consolidation or unloading Wrath does not disable this
  evaluator. Wrath options are copied read-only; otherwise upstream simple
  modes are selected. Blue Mage uses its active spellbook and configured DPS
  options rather than pretending it knows unequipped spells.
- When a live Wrath replacement is active at the evaluated entry point, its
  output takes priority. Diagnostics compare that output with the independent
  evaluator. A loaded plugin alone is not treated as proof of consolidation.
- Smaller icons are conditional basic-combo continuations, not a full forward
  simulation of future procs, buffs, cooldowns, or Wrath decisions. Unrelated
  burst actions no longer generate fabricated combo sequences.
- Debug mode exposes live source and parity state in settings-open tooltips.

## Commands

- `/vrh` opens settings.
- `/vrh toggle` enables or disables the suggestion bars.

All three bars open automatically in separate default positions and are visible
out of combat by default so they can be positioned immediately. Each fixed bar
and the dynamic bar can be toggled separately.
Keyboard hotkeys are read from the current character's hotbar labels and
refresh within 100 ms after layout changes. Each displayed key must currently
resolve to that action: consolidated buttons retain their key; separate actions
use their separate keys; native upgrades and transformations retain the actual
working button. Unbound/unknown actions have no invented hotkey. Unlock bars to
drag them into place.

## Upstream maintenance

Wrath provenance is pinned in `upstream/wrath.lock.json`. Run
`scripts/Audit-WrathUpdate.ps1` with a newer Wrath checkout to produce the
all-job compatibility report before integrating an update. See
`docs/WRATH_INTEGRATION.md`.

The authorized Hilda display reference is pinned separately in
`upstream/hilda.lock.json`; see `docs/HILDA_REFERENCE.md`.
