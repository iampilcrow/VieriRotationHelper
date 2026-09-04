using Dalamud.Plugin.Services;

namespace VieriRotationHelper;

internal sealed class ActionDisplay(IDataManager dataManager)
{
    internal (string Name, uint Icon) Get(uint actionId)
    {
        var row = dataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId);
        if (!row.HasValue)
            return ($"Action {actionId}", 0);
        return (row.Value.Name.ToString(), row.Value.Icon);
    }
}
