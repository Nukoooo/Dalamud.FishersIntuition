namespace FishersIntuition;

internal partial class Timers : IDisposable
{
    private bool _disposed;

    public Timers()
    {
        DalamudApi.GameInterop.InitializeFromAttributes(this);

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