using FishersIntuition.Utils;
using FishersIntuition.Windows;

namespace FishersIntuition;

internal class Plugin
{
    internal static Configuration Configuration { get; set; } = null!;
    internal static ConfigWindow ConfigWindow { get; set; } = null!;
    internal static TimerWindow TimerWindow { get; set; } = null!;
    internal static Timers Timers { get; set; } = null!;
    internal static SoundEngine SoundEngine { get; set; } = null!;

    public static void Initialize()
    {
        ConfigWindow = new ConfigWindow();
        TimerWindow = new TimerWindow();

        Configuration = (Configuration)DalamudApi.Interface.GetPluginConfig() ?? DalamudApi.Interface.Create<Configuration>();
        Timers = new Timers();
        SoundEngine = new SoundEngine();
    }
}