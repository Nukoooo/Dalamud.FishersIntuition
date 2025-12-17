namespace FishersIntuition;

internal partial class Timers : IDisposable
{
    private bool _disposed;

    public unsafe Timers()
    {
        if (!DalamudApi.SigScanner
                       .TryScanText("40 55 56 41 54 41 55 41 57 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 ?? 49 8B F1",
                                    out var processSystemLogMessagePacketAddress))
        {
            throw new NullReferenceException("Failed to find ProcessSystemLogMessagePacket");
        }

        if (!DalamudApi.SigScanner
                       .TryScanText("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 8B 81 ?? ?? ?? ?? 41 0F B7 F1",
                                    out var processEventPlayPacketAddress))
        {
            throw new NullReferenceException("Failed to find ProcessEventPlayPacket");
        }

        if (!DalamudApi.SigScanner
                       .TryScanText("84 D2 74 ?? 80 79 ?? ?? 88 51",
                                    out var updateWeatherHookAddress))
        {
            throw new NullReferenceException("Failed to find UpdateWeatherHook");
        }

        if (!DalamudApi.SigScanner
                       .TryScanText("83 FA ?? 0F 87 ?? ?? ?? ?? 48 89 5C 24 ?? 57 48 83 EC ?? 48 8B 05",
                                    out var oceanFishingInstanceContentUpdateHookAddress))
        {
            throw new NullReferenceException("Failed to find OceanFishingInstanceContentUpdateHook");
        }

        ProcessSystemLogMessagePacketHook
            = DalamudApi.GameInterop
                        .HookFromAddress<ProcessSystemLogMessagePacketDelegate>(processSystemLogMessagePacketAddress,
                                                                                hk_ProcessSystemLogMessagePacket);

        ProcessEventPlayPacketHook
            = DalamudApi.GameInterop.HookFromAddress<ProcessEventPlayPacketDelegate>(processEventPlayPacketAddress,
                                                                                         hk_ProcessEventPlayPacket);

        UpdateWeatherHook
            = DalamudApi.GameInterop.HookFromAddress<UpdateWeatherDelegate>(updateWeatherHookAddress, hk_UpdateWeather);

        OceanFishingInstanceContentUpdateHook
            = DalamudApi.GameInterop
                        .HookFromAddress<
                            OceanFishingInstanceContentUpdateDelegate>(oceanFishingInstanceContentUpdateHookAddress,
                                                                       hk_OceanFishingInstanceContentUpdate);

        ProcessSystemLogMessagePacketHook.Enable();
        ProcessEventPlayPacketHook.Enable();
        UpdateWeatherHook.Enable();
        OceanFishingInstanceContentUpdateHook.Enable();

        DalamudApi.Condition.ConditionChange += OnConditionChanged;
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