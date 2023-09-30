using System;
using System.IO;
using Dalamud.Utility.Signatures;

namespace FishersIntuition;

internal partial class Timers : IDisposable
{
    private bool _disposed;

    public unsafe Timers()
    {
        SignatureHelper.Initialise(this);

        if (!DalamudApi.SigScanner.TryGetStaticAddressFromSig("3B 05 ?? ?? ?? ?? 75 ?? C6 43", out var address))
        {
            throw new InvalidDataException("Failed to get address for current bite");
        }

        _currentBitePtr = (uint*)address;

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