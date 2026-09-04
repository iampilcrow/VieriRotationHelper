using Dalamud.Plugin.Services;

namespace VieriRotationHelper;

internal sealed class RotationCoordinator(
    IObjectTable objectTable,
    WrathLiveProvider wrath,
    EmbeddedRotationProvider embedded,
    TargetAnalysis targets,
    Configuration configuration)
{
    internal RotationFrame Evaluate(RotationMode mode)
    {
        var player = objectTable.LocalPlayer;
        if (player == null)
            return new(null, null, [], wrath.IsLoaded, false, mode, 0, 0, "Waiting for a character.");

        var classJob = (byte)player.ClassJob.RowId;
        if (!JobCatalog.TryGet(classJob, out var anchor))
            return new(null, null, [], wrath.IsLoaded, false, mode, 0, 0,
                $"Class/job {classJob} is not a supported combat job.");

        var targetSnapshot = targets.Snapshot(anchor.AoeAction);
        var effectiveMode = mode == RotationMode.Dynamic
            ? (targetSnapshot.EnemyCount >= Math.Clamp(configuration.DynamicAoeTargetCount, 2, 8)
                ? RotationMode.Aoe
                : RotationMode.SingleTarget)
            : mode;

        var embeddedLead = embedded.GetLead(anchor, effectiveMode);
        var sourceAction = effectiveMode == RotationMode.SingleTarget
            ? anchor.SingleTargetAction
            : anchor.AoeAction;

        RotationSuggestion lead;
        var parity = false;
        if (wrath.IsLoaded)
        {
            var exact = wrath.GetAdjusted(sourceAction);
            lead = new RotationSuggestion(exact, effectiveMode, SuggestionSource.LiveWrath, true,
                "Exact action currently exposed by Wrath Combo for this rotation entry point.");
            parity = exact == embeddedLead.ActionId;
        }
        else
        {
            lead = embeddedLead;
        }

        var forecast = embedded.Forecast(anchor, effectiveMode, lead.ActionId,
            Math.Clamp(configuration.PredictionCount, 1, 10));
        var status = wrath.IsLoaded
            ? parity ? "LIVE WRATH · PARITY" : "LIVE WRATH · FORECAST DIFFERS"
            : "EMBEDDED VIERI";
        var actionEnemyCount = targets.Snapshot(lead.ActionId).EnemyCount;
        return new(anchor, lead, forecast, wrath.IsLoaded, parity, effectiveMode,
            targetSnapshot.EnemyCount, actionEnemyCount, status);
    }
}
