using Dalamud.Plugin.Services;

namespace VieriRotationHelper;

internal sealed class ActionDisplay(IDataManager dataManager)
{
    internal ActionInfo Get(uint actionId)
    {
        var row = dataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId);
        if (!row.HasValue)
            return new ActionInfo($"Action {actionId}", 0, 0, 0);
        return new ActionInfo(row.Value.Name.ToString(), row.Value.Icon, row.Value.Range, row.Value.EffectRange);
    }
}
