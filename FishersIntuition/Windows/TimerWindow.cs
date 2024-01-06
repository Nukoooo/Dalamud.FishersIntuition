using System;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using FishersIntuition.Utils;
using ImGuiNET;

namespace FishersIntuition.Windows;

internal class TimerWindow : Window
{
    private float _frac = 0;
    private float _progressBarHeight = 15;
    private static readonly char[] ColonZero = { ':', '0' };

    public TimerWindow() : base("FishersIntuitionTimerWindow")
    {
        IsOpen = true;
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new(150, 130),
            MaximumSize = new(600, 130)
        };
    }

    public override bool DrawConditions()
    {
        if (DalamudApi.ClientState.LocalPlayer == null)
            return false;

        if (Plugin.Configuration.IsEditing)
            return true;

        return DalamudApi.ClientState.LocalPlayer.ClassJob.Id == 18 && DalamudApi.Condition[ConditionFlag.Gathering];
    }

    public override void PreDraw()
    {
        Flags = ImGuiWindowFlags.NoBackground;
        if (!Plugin.Configuration.IsEditing)
        {
            Flags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoInputs;
        }

        _progressBarHeight = Plugin.Configuration.BarHeight;
    }

    public override void Draw()
    {
        if (Plugin.Configuration.IsEditing)
            DrawEditingBar();
        else
            DrawMain();
    }

    private void DrawProgressBar(ImDrawListPtr drawList, float progress, bool isIn, uint color,
                                 uint backgroundColor)
    {
        var currentPos = ImGui.GetWindowPos() + ImGui.GetCursorPos();

        var barSize = ImGui.GetWindowSize().X - 20;

        drawList.AddRectFilled(currentPos - new Vector2(1f, 1f),
                               currentPos + new Vector2(barSize, _progressBarHeight) + new Vector2(1f, 1f),
                               backgroundColor,
                               9);
        drawList.AddRectFilled(currentPos + new Vector2(1f, 1f),
                               currentPos + new Vector2(barSize, _progressBarHeight) - new Vector2(1f, 1f),
                               backgroundColor,
                               9);

        if (progress <= float.Epsilon)
            return;

        var drawSize = (isIn ? EasingFunctions.InQuad(progress) : EasingFunctions.OutCubic(progress)) * barSize;
        drawList.AddRectFilled(currentPos, currentPos + new Vector2(drawSize, _progressBarHeight), color, 9);
    }

    private void DrawEditingBar()
    {
        ImGui.TextUnformatted($"抛杆时间: {DateTime.Now.TimeOfDay:ss\\.fff}".TrimStart(ColonZero));
        var color = Plugin.ConfigWindow.GetEditingColor() switch
                    {
                        1 => Plugin.Configuration.WeakBiteColor,
                        2 => Plugin.Configuration.StrongBiteColor,
                        3 => Plugin.Configuration.LegendaryBiteColor,
                        _ => Plugin.Configuration.CastColor
                    };

        var drawList = ImGui.GetWindowDrawList();
        DrawProgressBar(drawList, _frac, true, ImGui.ColorConvertFloat4ToU32(color),
                        ImGui.ColorConvertFloat4ToU32(Plugin.Configuration.BarBackgroundColor));

        var style = ImGui.GetStyle();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + _progressBarHeight + style.ItemSpacing.Y);

        ImGui.TextUnformatted($@"天气剩余时间: {DateTime.Now.TimeOfDay:mm\:ss\.fff}".TrimStart(ColonZero));
        DrawProgressBar(drawList, _frac, true,
                        ImGui.ColorConvertFloat4ToU32(Plugin.Configuration.WeatherColor),
                        ImGui.ColorConvertFloat4ToU32(Plugin.Configuration.BarBackgroundColor));

        _frac += 0.01f;
        if (_frac >= 1)
            _frac = 0f;
    }

    private void DrawMain()
    {
        var (castProgress, castTime) = Plugin.Timers.GetCastTimerProgress();

        var drawList = ImGui.GetWindowDrawList();
        var biteType = Plugin.Timers.GetBiteType();
        var color = biteType switch
                    {
                        BiteType.Weak => Plugin.Configuration.WeakBiteColor,
                        BiteType.Strong => Plugin.Configuration.StrongBiteColor,
                        BiteType.Legendary => Plugin.Configuration.LegendaryBiteColor,
                        _ => Plugin.Configuration.CastColor
                    };
        var type = biteType switch
                   {
                       BiteType.Weak => " |  轻杆",
                       BiteType.Strong => " |  重杆",
                       BiteType.Legendary => " |  鱼王杆",
                       _ => "",
                   };

        ImGui.TextUnformatted($"抛杆时间: {castTime:ss\\.fff} {type}".TrimStart(ColonZero));

        DrawProgressBar(drawList, castProgress, false, ImGui.ColorConvertFloat4ToU32(color),
                        ImGui.ColorConvertFloat4ToU32(Plugin.Configuration.BarBackgroundColor));
        var style = ImGui.GetStyle();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + _progressBarHeight + style.ItemSpacing.Y);

        var (weatherProgress, weatherTime) = Plugin.Timers.GetWeatherDuration();
        if (weatherTime <= TimeSpan.Zero)
            return;

        ImGui.TextUnformatted($@"天气剩余时间: {weatherTime:mm\:ss\.fff}".TrimStart(ColonZero));
        DrawProgressBar(drawList, weatherProgress, true,
                        ImGui.ColorConvertFloat4ToU32(Plugin.Configuration.WeatherColor),
                        ImGui.ColorConvertFloat4ToU32(Plugin.Configuration.BarBackgroundColor));
    }
}