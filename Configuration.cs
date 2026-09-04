using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace VieriRotationHelper;

public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool Enabled = true;
    public bool ShowSingleTarget = true;
    public bool ShowAoe = true;
    public bool ShowDynamic = true;
    public bool ShowOutOfCombat = true;
    public bool ShowWithoutTarget = true;
    public bool ShowHotkeys = true;
    public bool ShowWeaveIcon = true;
    public bool LockBars;
    public bool Horizontal = true;
    public bool ShowCooldownSweep = true;
    public bool ShowPositionals = true;
    public bool ShowRangeFade = true;
    public bool ShowEnemyCount = true;
    public bool DebugMode;
    public int PredictionCount = 3;
    public int DynamicAoeTargetCount = 3;
    public float IconSize = 65f;
    public float FutureIconScale = 50f / 65f;
    public float IconSpacing = 3f;
    public float Opacity = 1f;
    public Vector4 BackgroundColor = new(0.094f, 0.094f, 0.094f, 0.75f);
    public Vector4 BorderColor = new(0.565f, 0.447f, 0.847f, 1f);

    private IDalamudPluginInterface? pluginInterface;

    internal void Initialize(IDalamudPluginInterface value)
    {
        pluginInterface = value;
        if (Version < 2)
        {
            ShowOutOfCombat = true;
            Version = 2;
            Save();
        }
        if (Version < 3)
        {
            ShowDynamic = true;
            ShowPositionals = true;
            ShowRangeFade = true;
            ShowEnemyCount = true;
            DynamicAoeTargetCount = 3;
            Version = 3;
            Save();
        }
        if (Version < 4)
        {
            Horizontal = true;
            ShowHotkeys = true;
            ShowWeaveIcon = true;
            IconSize = 65f;
            FutureIconScale = 50f / 65f;
            IconSpacing = 3f;
            PredictionCount = 3;
            Version = 4;
            Save();
        }
    }
    internal void Save() => pluginInterface?.SavePluginConfig(this);
}
