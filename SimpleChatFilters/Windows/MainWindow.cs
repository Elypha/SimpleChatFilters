using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Style;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using Lumina.Excel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Components;
using Dalamud.Interface;
using ImGuiNET;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System;


namespace SimpleChatFilters;

public class MainWindow : Window, IDisposable
{
    // private IDalamudTextureWrap goatImage;
    private Plugin plugin;

    private Vector4 textColourHq = new Vector4(247f, 202f, 111f, 255f) / 255f;
    private Vector4 textColourHqOnly = new Vector4(62f, 186f, 240f, 255f) / 255f;
    private Vector4 textColourHigherThanVendor = new Vector4(230f, 90f, 80f, 255f) / 255f;
    private Vector4 textColourWhite = new Vector4(1f, 1f, 1f, 1f);

    private string newRegex_RegexList = string.Empty;
    private string newRegex_TypedRegexList = string.Empty;
    private string newRegex_ToastRegexList = string.Empty;

    public MainWindow(Plugin plugin) : base(
        "SimpleChatFilters",
        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(720, 720);
        SizeCondition = ImGuiCond.FirstUseEver;

        this.plugin = plugin;
    }

    public void Dispose()
    {
    }

    public override void OnClose()
    {
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
        var fontsize = ImGui.GetFontSize();
        var suffix = $"###{plugin.Name}-";
        var _window_x = ImGui.GetContentRegionAvail().X;
        var _window_y = ImGui.GetContentRegionAvail().Y;


        if (!ImGui.BeginTabBar($"{suffix}MainWindow-tabs"))
        {
            return;
        }

        if (ImGui.BeginTabItem("Chat Filters"))
        {
            DrawChatFiltersTab();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Toast Filters"))
        {
            DrawToastFiltersTab();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    public void DrawChatFiltersTab()
    {
        var scale = ImGui.GetIO().FontGlobalScale;
        var fontsize = ImGui.GetFontSize();
        var suffix = $"###{plugin.Name}-";
        var _window_w = ImGui.GetContentRegionAvail().X;
        var _window_h = ImGui.GetContentRegionAvail().Y;
        var _input_w = _window_w * scale - ImGui.CalcTextSize($"{FontAwesomeIcon.Trash.ToIconString()}").X - ImGui.GetStyle().ItemSpacing.X * 3;
        var _w = 0f;
        var _h = 0f;
        var section = "";


        section = "DrawChatFiltersTab";
        ImGui.BeginChild($"{suffix}{section}-child", new Vector2(_window_w, _window_h * 0.4f), false);

        // ----------------- Switches -----------------

        // Enable
        var Enabled = plugin.Config.ChatFilterEnabled;
        if (ImGui.Checkbox($"Enable{suffix}{section}-enable", ref Enabled))
        {
            plugin.Config.ChatFilterEnabled = Enabled;
            plugin.Config.Save();
        }
        ImGuiComponents.HelpMarker(
            "Enable: Display recent history entries under current listings.\n" +
            "Disable: The above will not happen."
        );

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{(char)FontAwesomeIcon.Cog}{suffix}{section}-open-config"))
        {
            plugin.DrawConfigUI();
        }
        ImGui.PopFont();



        // ----------------- Filters -----------------
        section = "RegexFilters";
        ImGui.Text("Regex");
        ImGuiComponents.HelpMarker(
            "Use this to match all messages.\n" +
            "e.g., a message in the System channel will be:\n" +
            "Welcome to Eorzea!\n" +
            "The following regex will match it:\n" +
            "^Welcome.+!"
        );
        for (var i = 0; i < plugin.Config.RegexList.Count; i++)
        {
            ImGui.PushID($"{suffix}{section}-list-{i}");
            var _regex = plugin.Config.RegexList[i];
            ImGui.SetNextItemWidth(_input_w * 0.9f);
            if (ImGui.InputText($"{suffix}{section}-list", ref _regex, 500))
            {
                plugin.Config.RegexList[i] = _regex;
                plugin.Config.Save();
                plugin.SimpleChatHandler.CompileRegexFilters();
            }
            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}{suffix}{section}-del"))
            {
                plugin.Config.RegexList.RemoveAt(i--);
                plugin.Config.Save();
                plugin.SimpleChatHandler.CompileRegexFilters();
            }
            ImGui.PopFont();
            ImGui.PopID();
            if (i < 0) break;
        }
        ImGui.SetNextItemWidth(_input_w * 0.9f);
        ImGui.InputText($"{suffix}{section}-new", ref newRegex_RegexList, 500);
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}{suffix}{section}-add"))
        {
            plugin.Config.RegexList.Add(newRegex_RegexList);
            plugin.Config.Save();
            newRegex_RegexList = string.Empty;
        }
        ImGui.PopFont();



        section = "TypedRegexFilters";
        ImGui.Text("Regex with type");
        ImGuiComponents.HelpMarker(
            "Use this when you want to match the type name as well. The name of the type is added at the beginning.\n" +
            "e.g., a message of the System type will be:\n" +
            "System:Welcome to Eorzea!\n" +
            "The following regex will match it:\n" +
            "^System:Welcome.+!"
        );
        for (var i = 0; i < plugin.Config.TypedRegexList.Count; i++)
        {
            ImGui.PushID($"{suffix}{section}-list-{i}");
            var _regex = plugin.Config.TypedRegexList[i];
            ImGui.SetNextItemWidth(_input_w * 0.9f);
            if (ImGui.InputText($"{suffix}{section}-list", ref _regex, 500))
            {
                plugin.Config.TypedRegexList[i] = _regex;
                plugin.Config.Save();
                plugin.SimpleChatHandler.CompileRegexFilters();
            }
            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}{suffix}{section}-del"))
            {
                plugin.Config.TypedRegexList.RemoveAt(i--);
                plugin.Config.Save();
                plugin.SimpleChatHandler.CompileRegexFilters();
            }
            ImGui.PopFont();
            ImGui.PopID();
            if (i < 0) break;
        }
        ImGui.SetNextItemWidth(_input_w * 0.9f);
        ImGui.InputText($"{suffix}{section}-new", ref newRegex_TypedRegexList, 500);
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}{suffix}{section}-add"))
        {
            plugin.Config.TypedRegexList.Add(newRegex_TypedRegexList);
            plugin.Config.Save();
            newRegex_TypedRegexList = string.Empty;
        }
        ImGui.PopFont();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().ItemSpacing.Y);

        ImGui.EndChild();


        // ----------------- FilteredChatLog -----------------
        section = "FilteredChatLog";
        ImGui.TextColored(plugin.ImGuiHelper.titleColour, "Filtered Messages");
        ImGui.Separator();
        _w = ImGui.GetContentRegionAvail().X;
        _h = ImGui.GetContentRegionAvail().Y / 2f;

        ImGui.BeginChild($"{suffix}{section}-child", new Vector2(_w, _h), false);
        ImGui.Columns(2, $"{suffix}{section}-table");

        foreach (var message in plugin.SimpleChatHandler.FilteredChatLog)
        {
            ImGui.Text(message.Type.ToString());
            ImGui.NextColumn();
            plugin.ImGuiHelper.RenderSeString(message.Message);
            ImGui.NextColumn();
            ImGui.Separator();
        }

        ImGui.EndChild();
        ImGui.Columns(1);

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().ItemSpacing.Y);



        // ----------------- ChatLog -----------------
        section = "ChatLog";
        ImGui.TextColored(plugin.ImGuiHelper.titleColour, "Messages");
        ImGui.Separator();
        _w = ImGui.GetContentRegionAvail().X;
        _h = ImGui.GetContentRegionAvail().Y;

        ImGui.BeginChild($"{suffix}{section}-child", new Vector2(_w, _h), false);
        ImGui.Columns(2, $"{suffix}{section}-table");

        foreach (var message in plugin.SimpleChatHandler.ChatLog)
        {
            ImGui.Text(message.Type.ToString());
            ImGui.NextColumn();
            plugin.ImGuiHelper.RenderSeString(message.Message);
            ImGui.NextColumn();
            ImGui.Separator();
        }

        ImGui.EndChild();


    }

    public void DrawToastFiltersTab()
    {
        var scale = ImGui.GetIO().FontGlobalScale;
        var fontsize = ImGui.GetFontSize();
        var suffix = $"###{plugin.Name}-";
        var _window_w = ImGui.GetContentRegionAvail().X;
        var _window_h = ImGui.GetContentRegionAvail().Y;
        var _input_w = _window_w * scale - ImGui.CalcTextSize($"{FontAwesomeIcon.Trash.ToIconString()}").X - ImGui.GetStyle().ItemSpacing.X * 3;
        var _w = 0f;
        var _h = 0f;
        var section = "";


        section = "DrawToastFiltersTab";
        ImGui.BeginChild($"{suffix}{section}-config", new Vector2(_window_w, _window_h * 0.4f), false);

        // ----------------- Switches -----------------

        // Enable
        var Enabled = plugin.Config.ToastFilterEnabled;
        if (ImGui.Checkbox($"Enable{suffix}{section}-enable", ref Enabled))
        {
            plugin.Config.ToastFilterEnabled = Enabled;
            plugin.Config.Save();
        }
        ImGuiComponents.HelpMarker(
            "Enable: Display recent history entries under current listings.\n" +
            "Disable: The above will not happen."
        );

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{(char)FontAwesomeIcon.Cog}{suffix}{section}-open-config"))
        {
            plugin.DrawConfigUI();
        }
        ImGui.PopFont();


        // ----------------- Regex for toast -----------------
        section = "ToastRegexList";
        ImGui.Text("Regex for toast");
        ImGuiComponents.HelpMarker(
            "Use this to match all toast messages.\n" +
            "e.g., a toast message will be:\n" +
            "Welcome to Eorzea!\n" +
            "The following regex will match it:\n" +
            "^Welcome.+!"
        );
        for (var i = 0; i < plugin.Config.ToastRegexList.Count; i++)
        {
            ImGui.PushID($"{suffix}{section}-list-{i}");
            var _regex = plugin.Config.ToastRegexList[i];
            ImGui.SetNextItemWidth(_input_w * 0.9f);
            if (ImGui.InputText($"{suffix}{section}-list", ref _regex, 500))
            {
                plugin.Config.ToastRegexList[i] = _regex;
                plugin.Config.Save();
                plugin.SimpleToastHandler.CompileRegexFilters();
            }
            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}{suffix}{section}-del"))
            {
                plugin.Config.ToastRegexList.RemoveAt(i--);
                plugin.Config.Save();
                plugin.SimpleToastHandler.CompileRegexFilters();
            }
            ImGui.PopFont();
            ImGui.PopID();
            if (i < 0) break;
        }
        ImGui.SetNextItemWidth(_input_w * 0.9f);
        ImGui.InputText($"{suffix}{section}-new", ref newRegex_ToastRegexList, 500);
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}{suffix}{section}-add"))
        {
            plugin.Config.ToastRegexList.Add(newRegex_ToastRegexList);
            plugin.Config.Save();
            newRegex_ToastRegexList = string.Empty;
        }
        ImGui.PopFont();

        ImGui.EndChild();


        // ----------------- FilteredToastLog -----------------
        section = "FilteredToastLog";
        ImGui.TextColored(plugin.ImGuiHelper.titleColour, "Filtered Toast Messages");
        ImGui.Separator();
        _w = ImGui.GetContentRegionAvail().X;
        _h = ImGui.GetContentRegionAvail().Y / 2f;

        ImGui.BeginChild($"{suffix}{section}-child", new Vector2(_w, _h), false);

        foreach (var toast in plugin.SimpleToastHandler.FilteredToastLog)
        {
            ImGui.Text(toast.Text);
            ImGui.Separator();
        }

        ImGui.EndChild();

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().ItemSpacing.Y);



        // ----------------- ToastLog -----------------
        section = "ToastLog";
        ImGui.TextColored(plugin.ImGuiHelper.titleColour, "Toast Messages");
        ImGui.Separator();
        _w = ImGui.GetContentRegionAvail().X;
        _h = ImGui.GetContentRegionAvail().Y;

        ImGui.BeginChild($"{suffix}{section}-child", new Vector2(_w, _h), false);

        foreach (var toast in plugin.SimpleToastHandler.ToastLog)
        {
            ImGui.Text(toast.Text);
            ImGui.Separator();
        }

        ImGui.EndChild();
    }

}
