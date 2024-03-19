using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using System.Collections.Generic;
using System;


namespace SimpleChatFilters;

[Serializable]
public class SimpleChatFiltersConfig : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    // ----------------- General -----------------
    public bool ChatFilterEnabled { get; set; } = false;
    public bool ToastFilterEnabled { get; set; } = false;
    public bool EnableTheme { get; set; } = true;
    public List<string> RegexList { get; set; } = new List<string>();
    public List<string> TypedRegexList { get; set; } = new List<string>();
    public List<string> ToastRegexList { get; set; } = new List<string>();

    public XivChatType ChatLogChannel { get; set; } = XivChatType.None;


    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        this.pluginInterface!.SavePluginConfig(this);
    }
}
