using System.Diagnostics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

// ReSharper disable once CheckNamespace
namespace FishersIntuition;

internal enum BiteType
{
    None,
    Weak,
    Strong,
    Legendary,
}

internal partial class Timers
{
    private readonly Stopwatch _castTimer = new();

    private unsafe uint CurrentBite => UIState.Instance()->PlayerState.FishingBait;

    private readonly TimeSpan _maxFishTime = TimeSpan.FromSeconds(63);
    private TimeSpan _elapsedTime = TimeSpan.Zero;
    private BiteType _biteType = BiteType.None;

    private uint _fishSpotId = 0;
    private uint _surfaceSlapFishId = 0;
    private uint _moochFishId = 0;

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    private Hook<ProcessSystemLogMessagePacketDelegate> ProcessSystemLogMessagePacketHook { get; init; } = null!;

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    private Hook<ProcessEventPlayPacketDelegate> ProcessEventPlayPacketHook { get; init; } = null!;

    private unsafe void hk_ProcessSystemLogMessagePacket(nint a1, uint eventId, uint logId, nint a4, byte a5)
    {
        ProcessSystemLogMessagePacketHook.Original(a1, eventId, logId, a4, a5);

        if (eventId != 0x150001)
            return;

        var data = *(uint*)a4;

        switch (logId)
        {
            case 1110: // cast
            {
                _castTimer.Start();
                _biteType = BiteType.None;
                _fishSpotId = data;
                break;
            }
            case 1121: // mooch
            {
                _castTimer.Start();
                _biteType = BiteType.None;
                _moochFishId = data;
                break;
            }
            case 5506: // surface slap
            {
                _surfaceSlapFishId = data;
                break;
            }
        }
    }

    private unsafe void hk_ProcessEventPlayPacket(
        nint eventFrameworkPtr,
        nint a2,
        uint eventId,
        FishingStatus status,
        nint a5,
        nint networkPtr, byte a7)
    {
        ProcessEventPlayPacketHook.Original(eventFrameworkPtr, a2, eventId, status, a5, networkPtr, a7);
        if (eventId != 0x150001)
            return;

        var data = *(uint*)networkPtr;

        switch (status)
        {
            case FishingStatus.Bite:
            {
                _elapsedTime = _castTimer.Elapsed;
                _castTimer.Reset();

                _biteType = data switch
                            {
                                292 => BiteType.Weak,
                                293 => BiteType.Strong,
                                294 => BiteType.Legendary,
                                _ => BiteType.None
                            };
                PlaySound();
                DalamudApi.PluginLog.Debug($"Bite {_elapsedTime.TotalMilliseconds}");
                break;
            }
            case FishingStatus.Finish:
            {
                if (_castTimer.IsRunning)
                {
                    _elapsedTime = _castTimer.Elapsed;
                    _castTimer.Reset();
                    DalamudApi.PluginLog.Debug($"Finish {_elapsedTime.TotalMilliseconds}");
                }

                break;
            }
        }
    }

    private void OnConditionChanged(ConditionFlag flag, bool value)
    {
        if (flag != ConditionFlag.Gathering || value)
            return;

        _elapsedTime = TimeSpan.Zero;
        _biteType = BiteType.None;
        _castTimer.Reset();
    }

    public (float, TimeSpan) GetCastTimerProgress()
    {
        return !_castTimer.IsRunning
            ? ((float)(_elapsedTime.TotalMilliseconds / _maxFishTime.TotalMilliseconds), _elapsedTime)
            : (_castTimer.ElapsedMilliseconds / (float)_maxFishTime.TotalMilliseconds, _castTimer.Elapsed);
    }

    public BiteType GetBiteType() => _biteType;

    private void PlaySound()
    {
        if (!Plugin.Configuration.PlaySound)
            return;

        string path;

        switch (_biteType)
        {
            case BiteType.Legendary:
            {
                path = string.IsNullOrWhiteSpace(Plugin.Configuration.LegendaryBiteSoundPath)
                    ? "Legendary bite"
                    : Plugin.Configuration.LegendaryBiteSoundPath;
                break;
            }
            case BiteType.Strong:
            {
                path = string.IsNullOrWhiteSpace(Plugin.Configuration.StrongBiteSoundPath)
                    ? "Strong bite"
                    : Plugin.Configuration.StrongBiteSoundPath;
                break;
            }
            case BiteType.Weak:
            {
                path = string.IsNullOrWhiteSpace(Plugin.Configuration.WeakBiteSoundPath)
                    ? "Weak bite"
                    : Plugin.Configuration.WeakBiteSoundPath;
                break;
            }
            case BiteType.None:
                return;
            default:
                return;
        }

        Plugin.SoundEngine.Play(path, Plugin.Configuration.Volume / 100f);
    }

    private enum FishingStatus : ushort
    {
        Cast = 1,
        Finish = 2,
        End = 3,
        Idle = 4,
        Bite = 5,
        BiteHook = 6
    }


    private delegate void ProcessSystemLogMessagePacketDelegate(nint a1, uint a2, uint a3, nint a4, byte a5);

    private delegate void ProcessEventPlayPacketDelegate(nint eventFrameworkPtr, nint a2, uint eventId,
                                                         FishingStatus status, nint a5,
                                                         nint networkPtr, byte a7);
}
