using Dalamud.Game.Text.SeStringHandling;
using System.Collections.Generic;
using Dalamud.Game.Text;
using System.Text.RegularExpressions;
using System.Linq;


namespace SimpleChatFilters;

public partial class SimpleChatHandler
{
    private Plugin plugin;

    public SimpleChatHandler(Plugin plugin)
    {
        this.plugin = plugin;

        CompileRegexFilters();
        Service.Chat.CheckMessageHandled += OnChat;
    }

    public void Dispose()
    {
        Service.Chat.CheckMessageHandled -= OnChat;
    }


    private List<Regex> regexList = new();
    private List<Regex> typedRegexList = new();
    public void CompileRegexFilters()
    {
        regexList = plugin.Config.RegexList.Select(regex => new Regex(regex, RegexOptions.Compiled)).ToList();
        typedRegexList = plugin.Config.TypedRegexList.Select(regex => new Regex(regex, RegexOptions.Compiled)).ToList();
    }


    public class ChatMessage
    {
        public ExtChatType.ChatType Type { get; set; }
        public SeString Message { get; set; } = new SeString();
        public string Text { get; set; } = string.Empty;
    }

    public List<ChatMessage> ChatLog { get; set; } = new();
    public List<ChatMessage> FilteredChatLog { get; set; } = new();




    public void OnChat(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (!plugin.Config.ChatFilterEnabled) return;
        if (isHandled) return;

        var chatType = ExtChatType.FromDalamud(type);

        // GM message
        if (GmMessage().IsMatch(chatType.ToString()))
        {
            Service.PluginLog.Warning($"[SimpleChatFilters] GM message: '{chatType}:{message.TextValue}'");
            return;
        }

        // Add message to ChatLog
        if (FilteredChatLog.Count >= 16) FilteredChatLog.RemoveAt(0);
        if (ChatLog.Count >= 16) ChatLog.RemoveAt(0);

        var chat_message = new ChatMessage
        {
            Type = chatType,
            Message = message,
            Text = $"{chatType}:{message.TextValue}"
        };

        // Match filters
        if (MatchRegexFilters(chat_message))
        {
            isHandled = true;
            FilteredChatLog.Add(chat_message);
        }
        else
        {
            ChatLog.Add(chat_message);
        }
    }

    private bool MatchRegexFilters(ChatMessage chat_message)
    {
        if (regexList.Any(regex => regex.IsMatch(chat_message.Text))) return true;
        if (typedRegexList.Any(regex => regex.IsMatch(chat_message.Text))) return true;
        return false;
    }


    // ----------------- Regex -----------------
    [GeneratedRegex(@"^Gm")]
    private static partial Regex GmMessage();
}
