using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using VieriRotationHelper.Windows;

namespace VieriRotationHelper;

public sealed class Plugin : IDalamudPlugin
{
    private const string Command = "/vrh";

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    internal Configuration Configuration { get; }
    private readonly WindowSystem windows = new("VieriRotationHelper");
    private readonly SettingsWindow settingsWindow;
    private readonly EmbeddedRotationProvider engine;
    internal OverlayFonts Fonts { get; }
    internal HotkeyResolver Hotkeys { get; }
    internal bool SettingsOpen => settingsWindow.IsOpen;
    internal string EngineStatus => engine.Status;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);
        Fonts = new OverlayFonts();
        Hotkeys = new HotkeyResolver(GameGui);

        var wrath = new WrathLiveProvider(PluginInterface);
        engine = new EmbeddedRotationProvider(this, wrath);
        var coordinator = new RotationCoordinator(ObjectTable, wrath, engine,
            new TargetAnalysis(ObjectTable, TargetManager, DataManager), Configuration);
        var display = new ActionDisplay(DataManager);

        windows.AddWindow(new RotationBarWindow(this, coordinator, display, RotationMode.SingleTarget));
        windows.AddWindow(new RotationBarWindow(this, coordinator, display, RotationMode.Aoe));
        windows.AddWindow(new RotationBarWindow(this, coordinator, display, RotationMode.Dynamic));
        settingsWindow = new SettingsWindow(this, wrath);
        windows.AddWindow(settingsWindow);

        CommandManager.AddHandler(Command, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open VieriRotationHelper settings. Use '/vrh toggle' to show or hide suggestions.",
        });
        PluginInterface.UiBuilder.Draw += windows.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += OpenSettings;
        PluginInterface.UiBuilder.OpenMainUi += OpenSettings;
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= windows.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenSettings;
        PluginInterface.UiBuilder.OpenMainUi -= OpenSettings;
        CommandManager.RemoveHandler(Command);
        windows.RemoveAllWindows();
        engine.Dispose();
        Fonts.Dispose();
        Configuration.Save();
    }

    internal bool ShouldShow(RotationMode mode)
    {
        if (!Configuration.Enabled || !ClientState.IsLoggedIn)
            return false;
        if (mode == RotationMode.SingleTarget && !Configuration.ShowSingleTarget)
            return false;
        if (mode == RotationMode.Aoe && !Configuration.ShowAoe)
            return false;
        if (mode == RotationMode.Dynamic && !Configuration.ShowDynamic)
            return false;
        if (!Configuration.ShowOutOfCombat && !Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
            return false;
        if (!Configuration.ShowWithoutTarget && TargetManager.Target == null)
            return false;
        return true;
    }

    internal void Save() => Configuration.Save();
    internal void OpenSettings() => settingsWindow.IsOpen = true;

    private void OnCommand(string command, string arguments)
    {
        if (arguments.Trim().Equals("toggle", StringComparison.OrdinalIgnoreCase))
        {
            Configuration.Enabled = !Configuration.Enabled;
            Save();
            ChatGui.Print($"[VieriRotationHelper] Suggestions {(Configuration.Enabled ? "enabled" : "disabled")}.");
            return;
        }
        OpenSettings();
    }
}
