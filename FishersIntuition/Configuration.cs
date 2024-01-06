using System.Numerics;
using Dalamud.Configuration;

namespace FishersIntuition;

internal class Configuration : IPluginConfiguration
{
    int IPluginConfiguration.Version { get; set; }

    public bool IsEditing { get; set; } = true;

    public Vector4 CastColor { get; set; } = new(0.31805545f, 0.75663716f, 0.38879445f, 0.95686275f);
    public Vector4 WeakBiteColor { get; set; } = new(0.23081426f, 0.69735885f, 0.733871f, 0.95686275f);
    public Vector4 StrongBiteColor { get; set; } = new(0.84070796f, 0.5319524f, 0.74857926f, 0.95686275f);
    public Vector4 LegendaryBiteColor { get; set; } = new(0.8584071f, 0.42313853f, 0.087360024f, 0.95686275f);
    public Vector4 WeatherColor { get; set; } = new(0.96902657f, 0.38589552f, 0.41685846f, 0.95686275f);
    public Vector4 BarBackgroundColor { get; set; } = new(0.13716817f, 0.13716817f, 0.13716817f, 0.49803922f);
    public float BarHeight { get; set; } = 15f;

    public bool PlaySound { get; set; } = false;
    public float Volume { get; set; } = 50f;

    public string WeakBiteSoundPath { get; set; } = string.Empty;
    public string StrongBiteSoundPath { get; set; } = string.Empty;
    public string LegendaryBiteSoundPath { get; set; } = string.Empty;

    public void Save()
    {
        DalamudApi.Interface.SavePluginConfig(this);
    }
}