#pragma warning disable CS8602

using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System;


namespace SimpleChatFilters;

public class ConfigWindow : Window, IDisposable
{

    private Plugin plugin;

    public ConfigWindow(Plugin plugin) : base(
        "SimpleChatFilters Configuration"
    // ImGuiWindowFlags.NoResize |
    // ImGuiWindowFlags.NoCollapse |
    // ImGuiWindowFlags.NoScrollbar |
    // ImGuiWindowFlags.NoScrollWithMouse
    )
    {
        Size = new Vector2(400, 300);
        SizeCondition = ImGuiCond.FirstUseEver;

        this.plugin = plugin;
    }


    public void Dispose()
    {
    }

    public override void OnOpen()
    {
    }


    public override void OnClose()
    {
        plugin.Config.Save();
    }


    public override void PreDraw()
    {
        if (plugin.Config.EnableTheme)
        {
            plugin.PluginTheme.Push();
            plugin.PluginThemeEnabled = true;
        }
    }

    public override void PostDraw()
    {
        if (plugin.PluginThemeEnabled)
        {
            plugin.PluginTheme.Pop();
            plugin.PluginThemeEnabled = false;
        }
    }


    public override void Draw()
    {
        var scale = ImGui.GetIO().FontGlobalScale;
        var padding = 0.8f;
        var fontsize = ImGui.GetFontSize();
        var titleColour = new Vector4(0.9f, 0.7f, 0.55f, 1);


        var suffix = $"###{plugin.Name}-";


        if (ImGui.CollapsingHeader("Features & UI Introduction"))
        {
            ImGui.TextColored(
                new Vector4(245f, 220f, 80f, 255f) / 255f,
                "> Below is a detailed manual.\n" +
                "> I recommend to read only the sections you are interested.\n" +
                "> Please checkout the changelog so that you can keep up with all the new features."
            );

            if (ImGui.Button($"Open Changelog Window"))
            {
                plugin.ChangelogWindow.Toggle();
            }

            ImGui.Text("");

            plugin.ImGuiHelper.BulletTextList(
                "Keybinding",
                "you can configure it below",
                new List<string> {
                    "All these can be configured with an optional delay.",
                    "Remember you can search multiple items without waiting for the previous ones to finish. All your query will be added to the cache sequentially.",
                }
            );


            ImGui.Text("");

            ImGui.TextColored(titleColour, "PS");
            var _ending_width = ImGui.GetContentRegionAvail().X;
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + _ending_width * 0.9f);
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + _ending_width * 0.05f);
            ImGui.TextColored(
                titleColour,
                "This plugin is still in active development.\n" +
                "If you have any suggestions or issues, please let me know on Discord or GitHub, and I will appreciate it.\n" +
                "Elypha."
            );
            ImGui.PopTextWrapPos();

            ImGui.Text("");

            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X / 2 - ImGui.CalcTextSize("- END -").X / 2);
            ImGui.TextColored(titleColour, "- END -");
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (4 * ImGui.GetTextLineHeight()));
        }

        // ----------------- General -----------------
        ImGui.TextColored(titleColour, "General");
        ImGui.Separator();


        // // Enabled
        // var Enabled = plugin.Config.ChatFilterEnabled;
        // if (ImGui.Checkbox($"Enable{suffix}Enabled", ref Enabled))
        // {
        //     plugin.Config.ChatFilterEnabled = Enabled;
        //     plugin.Config.Save();
        // }
        // ImGuiComponents.HelpMarker(
        //     "Enable: Display recent history entries under current listings.\n" +
        //     "Disable: The above will not happen."
        // );

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (padding * ImGui.GetTextLineHeight()));

    }
}
