using System;
using System.IO;
using Dalamud.Utility.Signatures;

namespace FishersIntuition;

internal partial class Timers : IDisposable
{
    private bool _disposed;

    public Timers()
    {
        DalamudApi.GameInterop.InitializeFromAttributes(this);

        DalamudApi.Condition.ConditionChange += OnConditionChanged;

        ProcessSystemLogMessagePacketHook.Enable();
        ProcessEventPlayPacketHook.Enable();
        UpdateWeatherHook.Enable();
        OceanFishingInstanceContentUpdateHook.Enable();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        DalamudApi.Condition.ConditionChange -= OnConditionChanged;
        ProcessSystemLogMessagePacketHook?.Dispose();
        ProcessEventPlayPacketHook?.Dispose();
        UpdateWeatherHook?.Dispose();
        OceanFishingInstanceContentUpdateHook?.Dispose();
    }
}