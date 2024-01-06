using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Interface.ImGuiFileDialog;

namespace FishersIntuition.Windows;

public class ConfigWindow : Window
{
    private byte _editingColor;
    private FileDialogManager _fileDialogManager;

    public ConfigWindow() : base("FishersIntuitionConfigWindow")
    {
        SizeCondition = ImGuiCond.FirstUseEver;
        _fileDialogManager = new();
    }

    public override void PreDraw()
    {
        Flags = ImGuiWindowFlags.AlwaysAutoResize;
    }

    public override void Draw()
    {
        ImGui.BeginTabBar("TabBar##Fishers");

        if (ImGui.BeginTabItem("Progress bar"))
        {
            DrawProgressBarTab();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Sound"))
        {
            DrawSoundTab();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    private void DrawProgressBarTab()
    {
        var editing = Plugin.Configuration.IsEditing;
        if (ImGui.Checkbox("Edit timer bar", ref editing))
        {
            Plugin.Configuration.IsEditing = editing;
        }

        var castColor = Plugin.Configuration.CastColor;
        if (ImGui.ColorEdit4("Cast color", ref castColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoOptions))
        {
            _editingColor = 0;
            Plugin.Configuration.CastColor = castColor;
        }

        var weakBiteColor = Plugin.Configuration.WeakBiteColor;
        if (ImGui.ColorEdit4("Weak bite color", ref weakBiteColor, ImGuiColorEditFlags.NoInputs))
        {
            _editingColor = 1;
            Plugin.Configuration.WeakBiteColor = weakBiteColor;
        }

        var strongBiteColor = Plugin.Configuration.StrongBiteColor;
        if (ImGui.ColorEdit4("Strong bite color", ref strongBiteColor, ImGuiColorEditFlags.NoInputs))
        {
            _editingColor = 2;
            Plugin.Configuration.StrongBiteColor = strongBiteColor;
        }

        var legendaryBiteColor = Plugin.Configuration.LegendaryBiteColor;
        if (ImGui.ColorEdit4("Legendary bite color", ref legendaryBiteColor, ImGuiColorEditFlags.NoInputs))
        {
            _editingColor = 3;
            Plugin.Configuration.LegendaryBiteColor = legendaryBiteColor;
        }

        var weatherColor = Plugin.Configuration.WeatherColor;
        if (ImGui.ColorEdit4("Weather color", ref weatherColor, ImGuiColorEditFlags.NoInputs))
        {
            _editingColor = 4;
            Plugin.Configuration.WeatherColor = weatherColor;
        }

        var barBackgroundColor = Plugin.Configuration.BarBackgroundColor;
        if (ImGui.ColorEdit4("Bar background color", ref barBackgroundColor, ImGuiColorEditFlags.NoInputs))
        {
            Plugin.Configuration.BarBackgroundColor = barBackgroundColor;
        }

        ImGui.SetNextItemWidth(100);
        var height = Plugin.Configuration.BarHeight;
        if (ImGui.SliderFloat("Bar Height", ref height, 8, 20))
        {
            Plugin.Configuration.BarHeight = height;
        }
    }

    private void DrawSoundTab()
    {
        var playSound = Plugin.Configuration.PlaySound;
        if (ImGui.Checkbox("Play sound on bite", ref playSound))
        {
            Plugin.Configuration.PlaySound = playSound;
        }

        var volume = Plugin.Configuration.Volume;
        ImGui.SetNextItemWidth(ImGui.GetFrameHeight() * 5);
        if (ImGui.SliderFloat("Volume", ref volume, 0f, 100f))
        {
            Plugin.Configuration.Volume = volume;
        }
    }

    public byte GetEditingColor()
    {
        return _editingColor;
    }

    public override void OnClose()
    {
        Plugin.Configuration.Save();
    }
}