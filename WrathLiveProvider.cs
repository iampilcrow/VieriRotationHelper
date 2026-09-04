using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace VieriRotationHelper;

internal sealed class WrathLiveProvider(IDalamudPluginInterface pluginInterface)
{
    internal bool IsLoaded => pluginInterface.InstalledPlugins.Any(plugin =>
        plugin.InternalName.Equals("WrathCombo", StringComparison.OrdinalIgnoreCase) && plugin.IsLoaded);

    internal unsafe uint GetAdjusted(uint anchorAction)
    {
        var manager = ActionManager.Instance();
        return manager == null ? anchorAction : manager->GetAdjustedActionId(anchorAction);
    }
}
