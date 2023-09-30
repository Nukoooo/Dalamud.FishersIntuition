using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;

namespace FishersIntuition;

public class EntryPoint : IDalamudPlugin
{
    private readonly PluginCommandManager<EntryPoint> _commandManager;
    private readonly WindowSystem _windowSystem;

    public EntryPoint([RequiredVersion("1.0")] DalamudPluginInterface pi)
    {
        pi.Create<DalamudApi>();
        pi.Create<Plugin>();

        Plugin.Initialize();

        _windowSystem = new WindowSystem(typeof(EntryPoint).AssemblyQualifiedName);

        _windowSystem.AddWindow(Plugin.ConfigWindow);
        _windowSystem.AddWindow(Plugin.TimerWindow);

        DalamudApi.Interface.UiBuilder.Draw += _windowSystem.Draw;
        DalamudApi.Interface.UiBuilder.OpenConfigUi += OpenConfigUi;

        // Load all of our commands
        _commandManager = new PluginCommandManager<EntryPoint>(this);
    }

    public string Name => "FishersIntuition";

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void OpenConfigUi()
    {
        Plugin.ConfigWindow.IsOpen = true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        Plugin.Timers.Dispose();
        _commandManager.Dispose();
        _windowSystem.RemoveAllWindows();
        DalamudApi.Interface.UiBuilder.Draw -= _windowSystem.Draw;
        DalamudApi.Interface.UiBuilder.OpenConfigUi -= OpenConfigUi;

        Plugin.Configuration.Save();
    }
}