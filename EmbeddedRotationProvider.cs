using FFXIVClientStructs.FFXIV.Client.Game;

namespace VieriRotationHelper;

internal sealed class EmbeddedRotationProvider
{
    internal unsafe RotationSuggestion GetLead(RotationAnchor anchor, RotationMode mode)
    {
        var sequence = mode == RotationMode.SingleTarget ? anchor.SingleTargetCombo : anchor.AoeCombo;
        var action = ResolveComboStep(sequence);
        return new RotationSuggestion(action, mode, SuggestionSource.EmbeddedVieri, true,
            "Embedded job rules selected the next usable combo action.");
    }

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
        if (index < 0)
            index = 0;

        for (var i = 1; i < count; i++)
        {
            var action = sequence[(index + i) % sequence.Length];
            result.Add(new RotationSuggestion(action, mode, SuggestionSource.EmbeddedVieri, false,
                "Forecast from the embedded simulated combo timeline."));
        }
        return result;
    }

    private static unsafe uint ResolveComboStep(uint[] sequence)
    {
        if (sequence.Length == 0)
            return 0;

        var manager = ActionManager.Instance();
        if (manager == null)
            return sequence[0];

        var last = manager->Combo.Action;
        if (manager->Combo.Timer <= 0)
            return manager->GetAdjustedActionId(sequence[0]);

        var index = Array.IndexOf(sequence, last);
        var next = index >= 0 && index + 1 < sequence.Length ? sequence[index + 1] : sequence[0];
        return manager->GetAdjustedActionId(next);
    }
}
