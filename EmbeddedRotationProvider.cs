using FFXIVClientStructs.FFXIV.Client.Game;

namespace VieriRotationHelper;

internal sealed class EmbeddedRotationProvider : IDisposable
{
    private readonly WrathCombo.ReadOnlyRuntime? runtime;
    private readonly WrathLiveProvider wrath;
    private readonly Configuration configuration;
    internal uint EntryAction { get; private set; }
    internal string Status { get; private set; } = "Waiting for a character.";
    private long nextErrorLog;

    internal EmbeddedRotationProvider(Plugin owner, WrathLiveProvider wrath)
    {
        this.wrath = wrath;
        configuration = owner.Configuration;
        try
        {
            runtime = new WrathCombo.ReadOnlyRuntime(Plugin.PluginInterface, owner, wrath.GetNativeAdjusted);
        }
        catch (Exception ex)
        {
            Status = "Decision engine initialization failed: " + ex.Message;
            Plugin.Log.Error(ex, Status);
        }
    }

    internal RotationSuggestion GetLead(RotationAnchor anchor, RotationMode mode)
    {
        try
        {
            if (runtime == null)
                return new(0, mode, SuggestionSource.EmbeddedVieri, false, Status);
            var decision = runtime.Evaluate(anchor.JobId, mode == RotationMode.Aoe, wrath.GetOptions(configuration));
            EntryAction = decision.EntryAction;
            Status = $"{anchor.Job}: {decision.Preset} — {decision.Detail}";
            return new(decision.ActionId, mode, SuggestionSource.EmbeddedVieri, true,
                $"{decision.Preset}: {decision.Detail}");
        }
        catch (Exception ex)
        {
            if (Environment.TickCount64 >= nextErrorLog)
            {
                nextErrorLog = Environment.TickCount64 + 10000;
                Plugin.Log.Error(ex, $"Independent Wrath evaluator failed for {anchor.Job}/{mode}.");
            }
            EntryAction = 0;
            Status = "Suggestions paused: " + ex.Message;
            return new(0, mode, SuggestionSource.EmbeddedVieri, false,
                "Suggestions paused: " + ex.Message);
        }
    }

    public void Dispose() => runtime?.Dispose();

    internal IReadOnlyList<RotationSuggestion> Forecast(
        RotationAnchor anchor,
        RotationMode mode,
        uint lead,
        int count)
    {
        if (count <= 1)
            return [];

        var sequence = mode == RotationMode.SingleTarget ? anchor.SingleTargetCombo : anchor.AoeCombo;
        if (sequence.Length <= 1)
            return [];

        var result = new List<RotationSuggestion>(count - 1);
        var index = Array.IndexOf(sequence, lead);
        // Never invent a basic combo continuation for an unrelated burst action.
        if (index < 0) return [];

        for (var i = 1; i < count; i++)
        {
            // Stop at the end rather than predicting another entire cycle.
            if (index + i >= sequence.Length) break;
            var action = wrath.GetNativeAdjusted(sequence[index + i]);
            if (!IsLearned(action)) break;
            result.Add(new RotationSuggestion(action, mode, SuggestionSource.EmbeddedVieri, false,
                "Conditional combo continuation, not a guaranteed future Wrath decision."));
        }
        return result;
    }

    private static unsafe bool IsLearned(uint action)
    {
        var manager = ActionManager.Instance();
        return manager != null && manager->GetActionStatus(ActionType.Action, action,
            checkRecastActive: false, checkCastingActive: false) != 573;
    }
}
