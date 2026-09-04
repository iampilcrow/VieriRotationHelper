using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace VieriRotationHelper;

// Hilda's keyboard hotbar-label lookup, with bounds/type checks and live
// adjusted-action matching for Wrath's replacement buttons.
internal sealed class HotkeyResolver(IGameGui gameGui)
{
    private readonly List<(uint Action, string Key)> bindings = [];
    private long nextRefresh;
    private ulong character;
    private uint job;

    internal unsafe string? Resolve(uint action, uint anchor, bool liveLead)
    {
        Refresh();
        var manager = ActionManager.Instance();
        // For the live lead, show the button whose replacement was evaluated.
        // Never claim a future action can be pressed on that button right now.
        if (liveLead && manager != null && manager->GetAdjustedActionId(anchor) == action)
        {
            foreach (var binding in bindings)
                if (binding.Action == anchor)
                    return binding.Key;
        }
        foreach (var binding in bindings)
            if (binding.Action == action)
                return binding.Key;
        if (manager != null)
            foreach (var binding in bindings)
                if (manager->GetAdjustedActionId(binding.Action) == action)
                    return binding.Key;
        foreach (var related in HildaHotkeyFamilies.Related(action))
            foreach (var binding in bindings)
                if (binding.Action == related)
                    return binding.Key;
        return null;
    }

    private unsafe void Refresh()
    {
        var player = Plugin.ObjectTable.LocalPlayer;
        if (player == null)
        {
            bindings.Clear();
            character = 0;
            return;
        }
        if (Environment.TickCount64 < nextRefresh && character == player.GameObjectId && job == player.ClassJob.RowId)
            return;
        nextRefresh = Environment.TickCount64 + 500;
        character = player.GameObjectId;
        job = player.ClassJob.RowId;
        bindings.Clear();
        var ui = UIModule.Instance();
        if (ui == null)
            return;
        var module = ui->GetRaptureHotbarModule();
        if (module == null)
            return;
        for (var bar = 0; bar < 10; bar++)
        {
            var name = bar == 0 ? "_ActionBar" : $"_ActionBar{bar:00}";
            var addon = (AtkUnitBase*)gameGui.GetAddonByName(name).Address;
            if (addon == null)
                continue;
            if (addon->UldManager.NodeList == null || addon->UldManager.NodeListCount <= 20)
                continue;
            // The displayed first hotbar may be paged to a different backing bar.
            var hotbarId = ((AddonActionBarBase*)addon)->RaptureHotbarId;
            if (hotbarId < 0 || hotbarId >= module->StandardHotbars.Length)
                continue;
            for (var slotIndex = 0; slotIndex < 12; slotIndex++)
            {
                var slot = module->StandardHotbars[(int)hotbarId].Slots[slotIndex];
                if (slot.CommandType != RaptureHotbarModule.HotbarSlotType.Action || slot.CommandId == 0)
                    continue;
                var node = addon->UldManager.NodeList[20 - slotIndex];
                if (node == null || (int)node->Type < 1000)
                    continue;
                var component = ((AtkComponentNode*)node)->Component;
                if (component == null || component->UldManager.NodeList == null || component->UldManager.NodeListCount < 2)
                    continue;
                var textNode = component->UldManager.NodeList[1];
                if (textNode == null || textNode->Type != NodeType.Text)
                    continue;
                var label = ((AtkTextNode*)textNode)->NodeText.ToString();
                if (!string.IsNullOrWhiteSpace(label))
                    bindings.Add((slot.CommandId, HotkeyLabel.Normalize(label)));
            }
        }
    }

}
