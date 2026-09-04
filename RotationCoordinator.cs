using Dalamud.Plugin.Services;

namespace VieriRotationHelper;

internal sealed class RotationCoordinator(
    IObjectTable objectTable,
    WrathLiveProvider wrath,
    EmbeddedRotationProvider embedded,
    Configuration configuration)
{
    internal RotationFrame Evaluate(RotationMode mode)
    {
        var player = objectTable.LocalPlayer;
        if (player == null)
            return new(null, null, [], wrath.IsLoaded, false, "Waiting for a character.");

        var classJob = (byte)player.ClassJob.RowId;
        if (!JobCatalog.TryGet(classJob, out var anchor))
            return new(null, null, [], wrath.IsLoaded, false,
                $"Class/job {classJob} is not a supported combat job.");

        var embeddedLead = embedded.GetLead(anchor, mode);
        var sourceAction = mode == RotationMode.SingleTarget
            ? anchor.SingleTargetAction
            : anchor.AoeAction;

        RotationSuggestion lead;
        var parity = false;
        if (wrath.IsLoaded)
        {
            var exact = wrath.GetAdjusted(sourceAction);
            lead = new RotationSuggestion(exact, mode, SuggestionSource.LiveWrath, true,
                "Exact action currently exposed by Wrath Combo for this rotation entry point.");
            parity = exact == embeddedLead.ActionId;
        }
        else
        {
            lead = embeddedLead;
        }

        var forecast = embedded.Forecast(anchor, mode, lead.ActionId,
            Math.Clamp(configuration.PredictionCount, 1, 10));
        var status = wrath.IsLoaded
            ? parity ? "LIVE WRATH · PARITY" : "LIVE WRATH · FORECAST DIFFERS"
            : "EMBEDDED VIERI";
        return new(anchor, lead, forecast, wrath.IsLoaded, parity, status);
    }
}
