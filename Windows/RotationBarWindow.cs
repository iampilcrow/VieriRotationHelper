using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace VieriRotationHelper.Windows;

internal sealed class RotationBarWindow : Window
{
    private readonly Plugin plugin;
    private readonly RotationCoordinator coordinator;
    private readonly ActionDisplay display;
    private readonly RotationMode mode;

    internal RotationBarWindow(
        Plugin plugin,
        RotationCoordinator coordinator,
        ActionDisplay display,
        RotationMode mode)
        : base($"Vieri Rotation · {ModeName(mode)}###VieriRotation{mode}",
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.plugin = plugin;
        this.coordinator = coordinator;
        this.display = display;
        this.mode = mode;
        IsOpen = true;
        ShowCloseButton = false;
        RespectCloseHotkey = false;
        Position = mode switch
        {
            RotationMode.SingleTarget => new Vector2(420, 260),
            RotationMode.Aoe => new Vector2(420, 390),
            _ => new Vector2(420, 520),
        };
        PositionCondition = ImGuiCond.FirstUseEver;
    }

    public override bool DrawConditions() => plugin.ShouldShow(mode);

    public override void PreDraw()
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize;
        if (plugin.Configuration.LockBars)
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, plugin.Configuration.Opacity);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, plugin.Configuration.BackgroundColor);
        ImGui.PushStyleColor(ImGuiCol.Border, plugin.Configuration.BorderColor);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor(2);
        ImGui.PopStyleVar();
    }

    public override void Draw()
    {
        var frame = coordinator.Evaluate(mode);
        if (frame.Lead == null)
        {
            ImGui.TextDisabled(frame.Status);
            return;
        }

        if (plugin.Configuration.ShowSourceBadge)
        {
            var color = frame.WrathLoaded ? new Vector4(0.48f, 0.95f, 0.62f, 1f) : new Vector4(0.75f, 0.68f, 1f, 1f);
            ImGui.TextColored(color, frame.Status);
        }
        if (mode == RotationMode.Dynamic)
        {
            if (plugin.Configuration.ShowSourceBadge)
                ImGui.SameLine();
            var dynamicColor = frame.EffectiveMode == RotationMode.Aoe
                ? new Vector4(1f, .58f, .30f, 1f)
                : new Vector4(.35f, .78f, 1f, 1f);
            ImGui.TextColored(dynamicColor,
                $"{(plugin.Configuration.ShowSourceBadge ? "· " : string.Empty)}{ModeName(frame.EffectiveMode).ToUpperInvariant()} ({frame.EnemyCount} nearby)");
        }

        DrawAction(frame.Lead, plugin.Configuration.IconSize, true, frame.ActionEnemyCount);
        foreach (var future in frame.Forecast)
        {
            if (plugin.Configuration.Horizontal)
                ImGui.SameLine();
            DrawAction(future, plugin.Configuration.IconSize * plugin.Configuration.FutureIconScale, false, frame.ActionEnemyCount);
        }

        DrawGcdIndicator(frame.Anchor);

        if (plugin.Configuration.DebugMode)
        {
            ImGui.Separator();
            ImGui.TextUnformatted($"Job: {frame.Anchor?.Job} · Mode: {ModeName(frame.EffectiveMode)} · Enemies: {frame.EnemyCount}");
            ImGui.TextWrapped(frame.Lead.Reason);
        }
    }

    private static string ModeName(RotationMode value) => value switch
    {
        RotationMode.SingleTarget => "Single Target",
        RotationMode.Aoe => "AoE",
        RotationMode.Dynamic => "Dynamic",
        _ => value.ToString(),
    };

    private static void DrawPlaceholder(float size)
    {
        ImGui.Dummy(new Vector2(size, size));
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        ImGui.GetWindowDrawList().AddRect(min, max, 0xFF707070);
    }

    private void DrawAction(RotationSuggestion suggestion, float size, bool lead, int enemyCount)
    {
        var info = display.Get(suggestion.ActionId);
        if (info.Icon == 0)
        {
            DrawPlaceholder(size);
        }
        else
        {
            var texture = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(info.Icon)).GetWrapOrEmpty();
            ImGui.Image(texture.Handle, new Vector2(size, size));
            DrawCooldown(suggestion.ActionId);
            DrawRangeFade(info);
            DrawPositional(suggestion.ActionId, size);
            if (lead)
                DrawEnemyCount(enemyCount, size);
            if (lead)
            {
                var min = ImGui.GetItemRectMin();
                var max = ImGui.GetItemRectMax();
                ImGui.GetWindowDrawList().AddRect(min - Vector2.One, max + Vector2.One,
                    ImGui.ColorConvertFloat4ToU32(plugin.Configuration.BorderColor), 3f, ImDrawFlags.None, 3f);
            }
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip($"{info.Name}\nAction {suggestion.ActionId}\n{suggestion.Reason}");
        if (plugin.Configuration.ShowActionNames)
        {
            var name = info.Name.Length > 16 ? info.Name[..15] + "…" : info.Name;
            ImGui.TextUnformatted(name);
        }
    }

    private static void DrawBadge(string text, Vector2 min, Vector2 max, uint background, uint foreground)
    {
        var draw = ImGui.GetWindowDrawList();
        var textSize = ImGui.CalcTextSize(text);
        draw.AddRectFilled(min, max, background, 3f);
        draw.AddText(min + (max - min - textSize) / 2f, foreground, text);
    }

    private void DrawPositional(uint actionId, float size)
    {
        if (!plugin.Configuration.ShowPositionals)
            return;
        var positional = PositionalCatalog.Get(actionId);
        if (positional == PositionalKind.None)
            return;

        var label = positional == PositionalKind.Flank ? "FLANK" : "REAR";
        var iconMin = ImGui.GetItemRectMin();
        var textSize = ImGui.CalcTextSize(label);
        var badgeSize = new Vector2(textSize.X + 8f, textSize.Y + 4f);
        var min = iconMin + new Vector2(Math.Max(0, size - badgeSize.X), Math.Max(0, size - badgeSize.Y));
        DrawBadge(label, min, min + badgeSize, 0xDD12301B, 0xFF67F58B);
    }

    private void DrawEnemyCount(int count, float size)
    {
        if (!plugin.Configuration.ShowEnemyCount || count <= 1)
            return;
        var iconMin = ImGui.GetItemRectMin();
        var label = $"x{count}";
        var textSize = ImGui.CalcTextSize(label);
        var badgeSize = new Vector2(textSize.X + 7f, textSize.Y + 4f);
        var min = iconMin + new Vector2(Math.Max(0, size - badgeSize.X), 0);
        DrawBadge(label, min, min + badgeSize, 0xDD111111, 0xFFFFFFFF);
    }

    private void DrawRangeFade(ActionInfo info)
    {
        if (!plugin.Configuration.ShowRangeFade || info.Range <= 0 || Plugin.TargetManager.Target == null)
            return;
        if (Plugin.TargetManager.Target.CurrentDistance <= info.Range)
            return;
        ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), 0x99000000);
    }

    private unsafe void DrawGcdIndicator(RotationAnchor? anchor)
    {
        if (!plugin.Configuration.ShowGcdIndicator || anchor == null)
            return;
        var manager = ActionManager.Instance();
        if (manager == null)
            return;
        var actionId = manager->GetAdjustedActionId(anchor.Value.SingleTargetAction);
        var total = manager->GetRecastTime(ActionType.Action, actionId);
        var elapsed = manager->GetRecastTimeElapsed(ActionType.Action, actionId);
        if (total <= .01f || elapsed <= .01f || elapsed >= total)
            return;

        var available = Math.Clamp(elapsed / total, 0f, 1f);
        var start = ImGui.GetCursorScreenPos();
        var width = Math.Max(plugin.Configuration.IconSize, ImGui.GetContentRegionAvail().X);
        var end = start + new Vector2(width, 4f);
        var draw = ImGui.GetWindowDrawList();
        draw.AddRectFilled(start, end, 0xAA242424, 2f);
        draw.AddRectFilled(start, new Vector2(start.X + width * available, end.Y), 0xFF9A73E8, 2f);
        ImGui.Dummy(new Vector2(width, 4f));
    }

    private unsafe void DrawCooldown(uint actionId)
    {
        if (!plugin.Configuration.ShowCooldownSweep)
            return;

        var manager = ActionManager.Instance();
        if (manager == null)
            return;

        var total = manager->GetRecastTime(ActionType.Action, actionId);
        var elapsed = manager->GetRecastTimeElapsed(ActionType.Action, actionId);
        if (total <= 0.01f || elapsed <= 0.01f || elapsed >= total)
            return;

        var remainingFraction = Math.Clamp((total - elapsed) / total, 0f, 1f);
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var coveredMax = new Vector2(max.X, min.Y + (max.Y - min.Y) * remainingFraction);
        ImGui.GetWindowDrawList().AddRectFilled(min, coveredMax, 0x99000000);

        var seconds = MathF.Max(0, total - elapsed);
        var text = seconds < 10 ? seconds.ToString("0.0") : MathF.Ceiling(seconds).ToString("0");
        var textSize = ImGui.CalcTextSize(text);
        ImGui.GetWindowDrawList().AddText(min + (max - min - textSize) / 2f, 0xFFFFFFFF, text);
    }
}
