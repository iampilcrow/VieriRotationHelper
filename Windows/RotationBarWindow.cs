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
        : base($"Vieri Rotation · {(mode == RotationMode.SingleTarget ? "Single Target" : "AoE")}###VieriRotation{mode}",
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.plugin = plugin;
        this.coordinator = coordinator;
        this.display = display;
        this.mode = mode;
        RespectCloseHotkey = false;
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

        DrawAction(frame.Lead, plugin.Configuration.IconSize, true);
        foreach (var future in frame.Forecast)
        {
            if (plugin.Configuration.Horizontal)
                ImGui.SameLine();
            DrawAction(future, plugin.Configuration.IconSize * plugin.Configuration.FutureIconScale, false);
        }

        if (plugin.Configuration.DebugMode)
        {
            ImGui.Separator();
            ImGui.TextUnformatted($"Job: {frame.Anchor?.Job} · Anchor: {(mode == RotationMode.SingleTarget ? frame.Anchor?.SingleTargetAction : frame.Anchor?.AoeAction)}");
            ImGui.TextWrapped(frame.Lead.Reason);
        }
    }

    private static void DrawPlaceholder(float size)
    {
        ImGui.Dummy(new Vector2(size, size));
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        ImGui.GetWindowDrawList().AddRect(min, max, 0xFF707070);
    }

    private void DrawAction(RotationSuggestion suggestion, float size, bool lead)
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
