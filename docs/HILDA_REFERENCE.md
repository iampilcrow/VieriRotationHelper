# Hilda display reference

The Hilda owner authorized the project to inspect and reuse Hilda behavior.
VieriRotationHelper 1.0.0.1 uses the pinned Hilda 7.5.1.0 assemblies as the
behavioral reference for:

- hostile, targetable, living-enemy counting around the player or target;
- action-radius-aware target counts;
- out-of-range icon dimming;
- flank and rear positional markers on action icons;
- main-icon enemy-count presentation; and
- cooldown and GCD timing feedback.

No Hilda assembly is redistributed or required at runtime. The source pin and
SHA-256 hashes are recorded in `upstream/hilda.lock.json` so a future Hilda
update can be audited deliberately without silently changing behavior.
