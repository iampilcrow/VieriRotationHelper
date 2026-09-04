# Wrath integration and update workflow

Wrath is both a live parity source and the reference for Vieri's embedded job
rules. The runtime integration does not call auto-rotation and never executes
an action. It reads the adjusted action IDs from Wrath's ST and AoE replacement
entry points.

## Source pin

`upstream/wrath.lock.json` records the exact upstream repository, version, and
commit used by the embedded rules. Do not change the pin without auditing and
testing every affected job.

## Updating

1. Check out the new Wrath source revision.
2. Run `scripts/Audit-WrathUpdate.ps1 -WrathPath <checkout>`.
3. Review every changed preset, action/status/gauge definition, job helper,
   simple/advanced rotation, and shared combat helper listed by the report.
4. Update `JobCatalog`, embedded job evaluators, and the source pin together.
5. Build and run tests. Every supported combat job must retain both ST and AoE
   mappings. Live-Wrath parity diagnostics must be clean in representative
   combat states before release.

The audit is deliberately machine-readable (`wrath-update-audit.json`) so a
future Codex request can identify exactly which jobs and shared systems changed
instead of manually rediscovering the integration surface.
