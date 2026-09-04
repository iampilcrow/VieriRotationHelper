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
    public bool ShowActionNames = true;
    public bool ShowSourceBadge = true;
    public bool LockBars;
    public bool Horizontal = true;
    public bool ShowCooldownSweep = true;
    public bool ShowGcdIndicator = true;
    public bool ShowPositionals = true;
    public bool ShowRangeFade = true;
    public bool ShowEnemyCount = true;
    public bool DebugMode;
    public int PredictionCount = 5;
    public int DynamicAoeTargetCount = 3;
    public float IconSize = 54f;
    public float FutureIconScale = 0.76f;
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
    }
    internal void Save() => pluginInterface?.SavePluginConfig(this);
}
