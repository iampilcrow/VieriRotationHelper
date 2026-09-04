using System.Numerics;
using VieriRotationHelper;

var checks = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    checks++;
}
Check(HildaVisualStyle.Position(0, 65, 50, 3, true) == new Vector2(20, 20), "Hilda lead origin");
Check(HildaVisualStyle.Position(1, 65, 50, 3, true) == new Vector2(91, 27.5f), "Hilda second icon and vertical centering");
Check(HildaVisualStyle.Position(2, 65, 50, 3, true) == new Vector2(147, 27.5f), "Hilda third icon spacing");
for (var count = 1; count <= 10; count++)
    foreach (var main in new[] { 32f, 65f, 96f })
        foreach (var ratio in new[] { .45f, 50f / 65f, 1f })
        {
            var secondary = main * ratio;
            for (var i = 1; i < count; i++)
            {
                var prev = HildaVisualStyle.Position(i - 1, main, secondary, 3, true);
                var next = HildaVisualStyle.Position(i, main, secondary, 3, true);
                var prevSize = i == 1 ? main : secondary;
                Check(next.X >= prev.X + prevSize, "Icons must remain side by side without overlap");
                Check(Math.Abs(next.Y + secondary / 2 - (20 * main / 65 + main / 2)) < .001f, "Center alignment");
            }
        }
for (var step = 0; step <= 80; step++)
{
    var uv = HildaVisualStyle.RecastUv(step / 80f, 1);
    Check(uv.Min.X >= 0 && uv.Min.Y >= 0 && uv.Max.X <= 1 && uv.Max.Y <= 1, "Recast sprite within texture bounds");
    Check(uv.Max.X > uv.Min.X && uv.Max.Y > uv.Min.Y, "Recast sprite must have positive dimensions");
}
Check(HildaVisualStyle.FlankSymbol == (char)62263 && HildaVisualStyle.RearSymbol == (char)62217, "Exact Hilda positional glyphs");
foreach (var pair in new[] { ("§1", "S1"), ("ª2", "A2"), ("¢3", "C3"), ("¾4", "CA4"), ("½5", "CS5"), ("¼6", "AS6"), ("¶7", "CAS7"), ("F12", "F12"), ("", "") })
    Check(HotkeyLabel.Normalize(pair.Item1) == pair.Item2, "Hilda hotkey normalization");
Console.WriteLine($"{checks} Hilda visual-contract checks passed.");
Check(HildaHotkeyFamilies.Groups.Length == 167, "All Hilda keyboard action families");
foreach (var group in HildaHotkeyFamilies.Groups)
    foreach (var action in group)
        Check(HildaHotkeyFamilies.Related(action).Contains(action), "Family lookup includes the action itself");
Check(HildaHotkeyFamilies.Related(16479).Contains(75u), "Raiden Thrust resolves True Thrust hotkey");
Check(!HildaHotkeyFamilies.Related(uint.MaxValue).Any(), "Unknown actions have no invented hotkeys");
Console.WriteLine($"{checks} total visual/hotkey checks passed.");
