using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace VieriRotationHelper.Windows;

internal sealed class SettingsWindow : Window
{
    private readonly Plugin plugin;
    private readonly WrathLiveProvider wrath;

    internal SettingsWindow(Plugin plugin, WrathLiveProvider wrath)
        : base("VieriRotationHelper Settings###VieriRotationHelperSettings")
    {
        this.plugin = plugin;
        this.wrath = wrath;
        Size = new Vector2(610, 700);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var changed = false;
        changed |= ImGui.Checkbox("Enable rotation suggestions", ref plugin.Configuration.Enabled);
        ImGui.SameLine();
        ImGui.TextColored(wrath.IsLoaded ? new Vector4(.35f, 1f, .5f, 1f) : new Vector4(1f, .75f, .3f, 1f),
            wrath.IsLoaded ? "Wrath: live parity source" : "Wrath: disabled · embedded source active");

        ImGui.Separator();
        ImGui.TextUnformatted("Suggestion bars");
        changed |= ImGui.Checkbox("Show Single Target", ref plugin.Configuration.ShowSingleTarget);
        changed |= ImGui.Checkbox("Show AoE", ref plugin.Configuration.ShowAoe);
        changed |= ImGui.Checkbox("Show Dynamic (switches by nearby targets)", ref plugin.Configuration.ShowDynamic);
        var aoeCount = plugin.Configuration.DynamicAoeTargetCount;
        if (ImGui.SliderInt("Dynamic AoE at target count", ref aoeCount, 2, 8))
        {
            plugin.Configuration.DynamicAoeTargetCount = aoeCount;
            changed = true;
        }
        changed |= ImGui.Checkbox("Show while out of combat", ref plugin.Configuration.ShowOutOfCombat);
        changed |= ImGui.Checkbox("Show without a target", ref plugin.Configuration.ShowWithoutTarget);
        changed |= ImGui.Checkbox("Show action names", ref plugin.Configuration.ShowActionNames);
        changed |= ImGui.Checkbox("Show source/parity badge", ref plugin.Configuration.ShowSourceBadge);
        changed |= ImGui.Checkbox("Lock bars", ref plugin.Configuration.LockBars);
        changed |= ImGui.Checkbox("Horizontal layout", ref plugin.Configuration.Horizontal);

        var predictions = plugin.Configuration.PredictionCount;
        if (ImGui.SliderInt("Actions shown", ref predictions, 1, 10))
        {
            plugin.Configuration.PredictionCount = predictions;
            changed = true;
        }
        changed |= ImGui.SliderFloat("Lead icon size", ref plugin.Configuration.IconSize, 32f, 96f, "%.0f px");
        changed |= ImGui.SliderFloat("Future icon scale", ref plugin.Configuration.FutureIconScale, .45f, 1f, "%.2f");
        changed |= ImGui.SliderFloat("Opacity", ref plugin.Configuration.Opacity, .2f, 1f, "%.2f");
        changed |= ImGui.ColorEdit4("Background", ref plugin.Configuration.BackgroundColor);
        changed |= ImGui.ColorEdit4("Border", ref plugin.Configuration.BorderColor);

        ImGui.Separator();
        ImGui.TextUnformatted("Timing and diagnostics");
        changed |= ImGui.Checkbox("Show cooldown sweep", ref plugin.Configuration.ShowCooldownSweep);
        changed |= ImGui.Checkbox("Show GCD indicator", ref plugin.Configuration.ShowGcdIndicator);
        changed |= ImGui.Checkbox("Show positional cues", ref plugin.Configuration.ShowPositionals);
        changed |= ImGui.Checkbox("Dim actions when target is out of range", ref plugin.Configuration.ShowRangeFade);
        changed |= ImGui.Checkbox("Show nearby enemy count", ref plugin.Configuration.ShowEnemyCount);
        changed |= ImGui.Checkbox("Debug state and parity", ref plugin.Configuration.DebugMode);

        ImGui.Spacing();
        ImGui.TextWrapped("The lead icon is authoritative. With Wrath loaded it is the exact action Wrath exposes for that bar's rotation entry point. Future icons are Vieri's simulated forecast and update immediately as live state changes. The plugin never presses or executes an action.");
        ImGui.Spacing();
        ImGui.TextDisabled("Command: /vrh  ·  /vrh toggle");

        if (changed)
            plugin.Save();
    }
}
