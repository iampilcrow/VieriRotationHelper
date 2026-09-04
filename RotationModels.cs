namespace VieriRotationHelper;

internal enum RotationMode
{
    SingleTarget,
    Aoe,
}

internal enum SuggestionSource
{
    None,
    LiveWrath,
    EmbeddedVieri,
}

internal readonly record struct RotationAnchor(
    byte JobId,
    string Job,
    uint SingleTargetAction,
    uint AoeAction,
    uint[] SingleTargetCombo,
    uint[] AoeCombo);

internal sealed record RotationSuggestion(
    uint ActionId,
    RotationMode Mode,
    SuggestionSource Source,
    bool Authoritative,
    string Reason);

internal sealed record RotationFrame(
    RotationAnchor? Anchor,
    RotationSuggestion? Lead,
    IReadOnlyList<RotationSuggestion> Forecast,
    bool WrathLoaded,
    bool ParityVerified,
    string Status);
