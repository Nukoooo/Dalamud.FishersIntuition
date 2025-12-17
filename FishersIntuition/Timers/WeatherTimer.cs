using System.Diagnostics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

// ReSharper disable once CheckNamespace
namespace FishersIntuition;

internal enum UpdateType : byte
{
    UpdateWeather = 1, // arg1 is weather id

    Setup = 3, // arg1 is CurrentRoute, arg2 is CurrentZone, arg3 is RemaingTime(always is 420), arg4 is TimeOffset(it is a timestamp for the time when the InstanceContent ends)
    ChangeZone = 4, // arg1 is new zone

    UpdateCutsceneStatus = 5, // only arg1 has value. 2 is cutscene finished, 3 is cutscene started, 1 is play new cutscene (after changing the zone)
    UpdateCurrnetZoneTime = 6, // arg1 is reaming time(and always is 420), arg2 is TimeOffset
    SpectralCurrentStart = 7, // arg1 is current start time offset?
    SpectralCurrentFinish = 8, // no values
    SpectralCurrentReset = 9, // this only happens after entering ocean fishing, and no values from args

    SetSpawnPlaceName = 10, // arg1 is row id for PlaceName sheet, not sure what arg2 is, but it is a boolean and this only happens right after entering ocean fishing
    UpdateMissionProgress = 12, // arg1 is for Mission1, so on and so forth
    UpdateTimeOffset = 13, // arg1 is the new time offset
}

internal partial class Timers
{
    private readonly byte[]    _specialWeathers = [133, 134, 135, 136, 145];
    private readonly Stopwatch _weatherTimer    = new ();
    private          TimeSpan  _weatherDuration = TimeSpan.Zero;

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    private Hook<UpdateWeatherDelegate> UpdateWeatherHook { get; init; } = null!;

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    private Hook<OceanFishingInstanceContentUpdateDelegate> OceanFishingInstanceContentUpdateHook { get; init; } =
        null!;

    private void hk_UpdateWeather(nint a1, byte weatherType, float a3, byte a4)
    {
        UpdateWeatherHook.Original(a1, weatherType, a3, a4);

        if (weatherType == 145)
            return;

        if (_specialWeathers.All(i => i != weatherType))
        {
            _weatherTimer.Reset();
            return;
        }

        _weatherTimer.Start();
        _weatherDuration = TimeSpan.FromSeconds(600);
    }

    private float _oceanFishingExtraTime;
    private bool _oceanFishingHasCurrent;
    private bool _oceanFishingLastZoneHasCurrent;

    private unsafe void hk_OceanFishingInstanceContentUpdate(InstanceContentOceanFishing* a1, UpdateType updateType,
                                                             int arg1, int arg2, int arg3, int arg4)
    {
        OceanFishingInstanceContentUpdateHook.Original(a1, updateType, arg1, arg2, arg3, arg4);

        switch (updateType)
        {
            case UpdateType.Setup:
            {
                _oceanFishingExtraTime = 0f;
                _oceanFishingLastZoneHasCurrent = false;
                _oceanFishingHasCurrent = false;
                return;
            }
            case UpdateType.ChangeZone:
            {
                _oceanFishingLastZoneHasCurrent = _oceanFishingHasCurrent;
                _oceanFishingHasCurrent = false;
                return;
            }
            case UpdateType.SpectralCurrentStart:
            {
                var duration = 120f;

                var timeLeft = a1->InstanceContentDirector.ContentDirector.ContentTimeLeft - a1->TimeOffset;

                if (a1->CurrentZone != 0 && !_oceanFishingLastZoneHasCurrent)
                    duration += 60f;
                else
                    duration += _oceanFishingExtraTime;

                _oceanFishingExtraTime = 0f;

                duration = Math.Min(duration, 180f);

                var endTime = timeLeft - duration;
                if (endTime <= 30f)
                {
                    _oceanFishingExtraTime = Math.Clamp(Math.Abs(timeLeft - 30f), 0f, 60f);

                    duration = timeLeft - 30;
                }

                DalamudApi.PluginLog.Debug($"SpectralCurrentStart. duration: {duration}, timeLeft: {timeLeft}, endTime: {endTime}, extraTime: {_oceanFishingExtraTime}");

                _oceanFishingHasCurrent = true;

                _weatherDuration = TimeSpan.FromSeconds(duration);
                _weatherTimer.Start();
                return;
            }
            case UpdateType.SpectralCurrentFinish:
            {
                _oceanFishingHasCurrent = true;
                _weatherTimer.Reset();
                return;
            }
        }
    }

    public (float, TimeSpan) GetWeatherDuration()
    {
        if (!_weatherTimer.IsRunning)
            return (0, TimeSpan.Zero);

        var progress = 1f - _weatherTimer.ElapsedMilliseconds / (float)_weatherDuration.TotalMilliseconds;
        if (progress > 0.0001f)
            return (progress, _weatherDuration - _weatherTimer.Elapsed);

        _weatherTimer.Reset();
        _weatherDuration = TimeSpan.Zero;
        return (0, TimeSpan.Zero);
    }

    private delegate void UpdateWeatherDelegate(nint a1, byte weatherType, float a3, byte a4);

    private unsafe delegate void OceanFishingInstanceContentUpdateDelegate(
        InstanceContentOceanFishing* a1, UpdateType updateType, int arg1, int arg2, int arg3, int arg4);
}