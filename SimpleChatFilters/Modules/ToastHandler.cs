using Dalamud.Game.Text.SeStringHandling;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Game.Gui.Toast;
using System.Linq;


namespace SimpleChatFilters;

public class SimpleToastHandler
{
    private Plugin plugin;

    public SimpleToastHandler(Plugin plugin)
    {
        this.plugin = plugin;

        CompileRegexFilters();
        Service.Toasts.Toast += OnToast;
        Service.Toasts.ErrorToast += OnErrorToast;
        Service.Toasts.QuestToast += OnQuestToast;
    }

    public void Dispose()
    {
        Service.Toasts.Toast -= OnToast;
        Service.Toasts.ErrorToast -= OnErrorToast;
        Service.Toasts.QuestToast -= OnQuestToast;
    }


    private List<Regex> toastRegexList = new();
    public void CompileRegexFilters()
    {
        toastRegexList = plugin.Config.ToastRegexList.Select(regex => new Regex(regex, RegexOptions.Compiled)).ToList();
    }


    public class ToastMessage
    {
        public SeString Message { get; set; } = new SeString();
        public string Text { get; set; } = string.Empty;
    }
    public List<ToastMessage> ToastLog { get; set; } = new();
    public List<ToastMessage> FilteredToastLog { get; set; } = new();

    public void OnToast(ref SeString message, ref ToastOptions options, ref bool isHandled)
    {
        ProcessToast(message, ref isHandled);
    }

    public void OnErrorToast(ref SeString message, ref bool isHandled)
    {
        ProcessToast(message, ref isHandled);
    }

    public void OnQuestToast(ref SeString message, ref QuestToastOptions options, ref bool isHandled)
    {
        ProcessToast(message, ref isHandled);
    }

    public void ProcessToast(SeString message, ref bool isHandled)
    {
        try
        {
            if (!plugin.Config.ToastFilterEnabled) return;
            if (isHandled) return;

            // Add message to ToastLog
            if (FilteredToastLog.Count >= 16) FilteredToastLog.RemoveAt(0);
            if (ToastLog.Count >= 16) ToastLog.RemoveAt(0);

            var toast_message = new ToastMessage
            {
                Message = message,
                Text = $"{message.TextValue}"
            };

            // Match filters
            if (MatchRegexFilters(toast_message))
            {
                isHandled = true;
                FilteredToastLog.Add(toast_message);
            }
            else
            {
                ToastLog.Add(toast_message);
            }
        }
        catch (System.Exception e)
        {
            Service.PluginLog.Error($"[SimpleChatFilters] Error in ProcessToast: {e}");
        }
    }

    private bool MatchRegexFilters(ToastMessage toast_message)
    {
        if (toastRegexList.Any(regex => regex.IsMatch(toast_message.Text))) return true;
        return false;
    }
}
