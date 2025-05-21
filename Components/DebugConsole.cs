using ImGuiNET;
using Raylib_cs;

public class DebugConsole
{
    const string Tag = "DebugConsole";
    const int MaxLines = 1000;

    CircularBuffer<string> buffer;
    Font font;
    int charWidth;
    bool scrollToBottom;
    bool[] severityToggles = [false, false, false, false, false, false];
    
    bool[] tagToggles;
    float maxTagWidth;
    List<string> tags;
    Dictionary<string, int> severityLookup;
    Dictionary<string, int> messageToTagIndex;
    Dictionary<string, int> tagToIndex;

    public DebugConsole()
    {
        buffer = new(MaxLines);
        tags = new();
        tagToggles = Array.Empty<bool>();
        severityLookup = new();
        messageToTagIndex = new();
        tagToIndex = new();
        Log.OnLogMessage += HandleLogMessage;
    }

    public void SetFont(Font font)
    {
        this.font = font;
        charWidth = Util.GetTextWidth(" ", font);
    }

    public void Draw()
    {
        ImGui.Begin("Debug Console");

        for (int i = 1; i < severityToggles.Length; i++)
        {
            ImGui.Checkbox(Log.SeverityTag[i], ref severityToggles[i]);
            if (i < severityToggles.Length - 1)
            {
                ImGui.SameLine();
            }
        }

        var style = ImGui.GetStyle();

        float windowWidth = ImGui.GetCursorPos().X + ImGui.GetContentRegionAvail().X;
        float x = 0;
        for (int i = 0; i < tags.Count; i++)
        {
            ImGui.PushID(i);
            ImGui.Checkbox(tags[i], ref tagToggles[i]);
            float buttonWidth = ImGui.GetItemRectSize().X;
            maxTagWidth = Math.Max(maxTagWidth, buttonWidth);
            x += buttonWidth + style.ItemSpacing.X;
            
            float nextButtonX = x + maxTagWidth + style.ItemSpacing.X;
            
            if (i < tags.Count - 1 && nextButtonX < windowWidth)
            {
                ImGui.SameLine();
            }
            else
            {
                x = 0;
            }
            ImGui.PopID();
        }

        ImGui.Separator();

        ImGui.BeginChild("Debug messages");
        foreach (var line in buffer)
        {
            if (line is null) continue;

            if (severityToggles.Any(x => x))
            {
                if (!severityLookup.TryGetValue(line, out int sev) || !severityToggles[sev])
                {
                    continue;
                }
            }

            if (tagToggles.Any(x => x))
            {
                if (!messageToTagIndex.TryGetValue(line, out int tag)) continue;
                if (!tagToggles[tag]) continue;
            }
            ImGui.TextWrapped(line);
            
            // This is supposed to scroll to the bottom, but it scrolls to the top??
            // the log message order is inverted so it works as expected 
            //if (scrollToBottom)
            //{
            //    ImGui.SetScrollHereY(1.0f);
            //    scrollToBottom = false;
            //}
        }
        ImGui.EndChild();
        ImGui.End();
    }

    void HandleLogMessage(Severity sev, string tag, string message, bool ignoreFilter)
    {
        buffer.Add(message);

        if (!tagToIndex.ContainsKey(tag))
        {
            tagToIndex.Add(tag, tags.Count);
            
            tags.Add(tag);
            tagToggles = [..tagToggles, false];
        }
        if (!messageToTagIndex.ContainsKey(message))
        {
            int index = tagToIndex[tag];
            messageToTagIndex.Add(message, index);
        }

        if (!severityLookup.ContainsKey(message)) 
        {
            severityLookup.Add(message, (int)sev);
        }
        scrollToBottom = true;
    }

    // For testing purposes
    void RandomMessages(float chance)
    {
        var rnd = new Random();
        if (rnd.NextDouble() < chance)
        {
            var tag = ('a' + rnd.Next(15)).ToString();
            var message = Guid.NewGuid().ToString();
            var sev = rnd.Next(5);
            switch (sev)
            {
                case 0: Log.Trace(tag, message); break;
                case 1: Log.Info(tag, message); break;
                case 2: Log.Warning(tag, message); break;
                case 3: Log.Error(tag, message); break;
                case 4: Log.Fatal(tag, message); break;
            }
        }
    }
}