using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using System.Numerics;
using System.Text;
using static Raylib_cs.Raylib;

public class PracticeScene : Scene
{
    const string Tag = "PracticeScene";

    Font primaryFont;
    Font secondaryFont;
    Font smallFont;
    int primaryCharWidth = 0;
    int smallCharWidth = 0;
    
    string lessonPath;
    List<Word> words;
    PloverServer server;
    Input input;
    WPM wpm;
    List<string> paper;

    bool complete = false;
    float timeSinceComplete = 0f;

    int wordIndex = 0;
    float xOffset = 0; // Used for horizontal smooth scrolling
    float smoothProgressPercent = 0;

    bool timerRunning = false;
    float timer = 0;
    float timeSinceType = 0;
    
    const float StopTimingThreshold = 5f;

    public PracticeScene(string lessonFilepath)
    {
        lessonPath = lessonFilepath;
        words = new();
        server = new();
        input = new(server);
        wpm = new();
        paper = new();

        input.OnBackspace += Backspace;
        input.OnTextTyped += TextTyped;

        server.OnSendStroke += Stroke;

        SetFontSize(60);
    }

    public void Load() 
    {
        var lesson = Lesson.Load(lessonPath);
        if (lesson is null)
        {
            Log.Error(Tag, "Unable to load lesson scene");
            throw new ApplicationException($"Unable to load lesson {lessonPath}", null);
        }
        words = lesson.Words ?? [];

        try
        {
            bool connected = server.Connect().GetAwaiter().GetResult();
            if (!connected)
            {
                input.SetInputMode(Input.Mode.Keyboard);
            }
        }
        catch
        {

        }

        input.SetInputMode(Input.Mode.Plover);
    }

    public void Unload() { }
    
    public void Draw() 
    {
        const float padding = 10f;

        ClearBackground(Shared.BackgroundColor);

        ImGui.ShowDemoWindow();

        if (ImGui.Begin("Simple Window"))
        {
            ImGui.TextUnformatted("Test " + IconFonts.FontAwesome6.BookAtlas);
        }

        ImGui.End();

        Vector2 origin = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2 - primaryFont.BaseSize);
        Vector2 cursor = origin;
        cursor.X += xOffset;

        int width = 0;

        // Draw cursor
        Word word = words[wordIndex];
        DrawRectangle((int)cursor.X + GetWordWidth(word, true), (int)cursor.Y, 2, primaryFont.BaseSize, Shared.AccentColor);
        
        // Draw words
        for (int i = wordIndex; i < words.Count; i++)
        {
            word = words[i];
            width = DrawWord(word, cursor, false);
            cursor.X += width;

            if (cursor.X > GetScreenWidth()) break;
        }

        cursor = origin;
        cursor.X += xOffset;
        for (int i = wordIndex - 1; i >= 0 && i < words.Count; i--)
        {
            word = words[i];
            cursor.X -= GetWordWidth(word);
            DrawWord(word, cursor, true);

            if (cursor.X < 0) break;
        }

        // Draw paper
        const int PaperChars = 23;
        const int PaperLines = 8;
        cursor.X = GetScreenWidth() - PaperChars * smallCharWidth;
        cursor.Y = (PaperLines - 1) * smallFont.BaseSize;
        for (int i = paper.Count - 1; i >= 0; i--)
        {
            Util.DrawText(smallFont, paper[i], cursor, Shared.AltTextColor);
            cursor.Y -= smallFont.BaseSize;
        }

        // Draw left panel
        width = primaryCharWidth * 8 + (int)padding * 2;
        DrawRectangle(0, 0, width, GetScreenHeight() + 1, Shared.PanelColor);

        Color timerColor = Shared.TextColor;
        if (timerRunning)
        {
            timerColor = Shared.AltTextColor;
        }
        
        // Draw WPM
        cursor.X = padding;
        cursor.Y = GetScreenHeight() - primaryFont.BaseSize - padding * 3;
        Util.DrawText(primaryFont, FormatWpm(wpm.GetWPM()), cursor, timerColor);

        // Draw timer
        string timerText = FormatTime(timer);
        cursor.X = padding;
        cursor.Y -= primaryFont.BaseSize + padding;
        Util.DrawText(primaryFont, timerText, cursor, timerColor);
        
        // Draw progress bar
        const int progressBarHeight = 20;
        float progressPercent = (float)wordIndex / words.Count;
        if (complete)
        {
            // If we are done, set progress to 100%
            progressPercent = 1;
        }
        smoothProgressPercent = Util.ExpDecay(smoothProgressPercent, progressPercent, 15, GetFrameTime());
        int progressPixels = (int)(GetScreenWidth() * smoothProgressPercent);
        DrawRectangle(0, GetScreenHeight() - progressBarHeight, progressPixels, progressBarHeight  + 1, Shared.AltTextColor);
    }

    public void Update() 
    {
        input.Update();

        timeSinceType += GetFrameTime();
        
        float dy = GetMouseWheelMoveV().Y;
        if (dy != 0)
        {
            int newFontSize = primaryFont.BaseSize + (int)dy;
            newFontSize = Math.Clamp(newFontSize, 20, 80);

            SetFontSize(newFontSize);
        }

        xOffset = Util.ExpDecay(xOffset, 0, 20, GetFrameTime());

        if (timeSinceType > StopTimingThreshold)
        {
            timerRunning = false;
        }

        if (timerRunning)
        {
            timer += GetFrameTime();
            wpm.Update();
        }
    }

    void SetFontSize(int fontSize)
    {
        primaryFont = Shared.GetFont(Shared.PrimaryFontFile, fontSize);
        secondaryFont = Shared.GetFont(Shared.SecondaryFontFile, fontSize);
        smallFont = Shared.GetFont(Shared.PrimaryFontFile, (int)(fontSize * .6f));

        // Assuming mono-spaced font, so character width will be consistent
        primaryCharWidth = Util.GetTextWidth(" ", primaryFont);
        smallCharWidth = Util.GetTextWidth(" ", smallFont);
    }

    void TextTyped(string text)
    {
        if (complete) return;

        timerRunning = true;
        timeSinceType = 0;

        foreach (var key in text)
        {
            KeyTyped(key);
        }
    }

    void KeyTyped(char key)
    {
        Word word = words[wordIndex];

        if (key == ' ')
        {
            if (word.InputBuffer.Length > 0)
            {
                if (!word.Counted)
                {
                    if (word.InputBuffer.ToString() == word.Text)
                    {
                        word.Counted = true;
                        wpm.WordTyped(word);
                    }
                }

                if (Shared.UserSettings.SmoothScroll && wordIndex < words.Count - 1)
                {
                    xOffset += GetWordWidth(words[wordIndex], false);
                }

                wordIndex = Math.Min(wordIndex + 1, words.Count - 1);
            }
        }
        else
        {
            words[wordIndex].InputBuffer.Append(key);

            // Check if we are done
            if (wordIndex == words.Count - 1 && words[wordIndex].InputBuffer.ToString() == words[wordIndex].Text)
            {
                complete = true;
                timerRunning = false;
            }
        }
    }

    void Backspace(int count)
    {
        if (complete) return;
        for (int i = 0; i < count; i++)
        {
            var word = words[wordIndex];
            if (word.InputBuffer.Length == 0)
            {
                // Go to previous word
                if (wordIndex > 0)
                {
                    wordIndex--;
                    if (Shared.UserSettings.SmoothScroll)
                    {
                        xOffset -= GetWordWidth(words[wordIndex], false);
                    }
                }
            }
            else
            {
                // Remove last character
                StringBuilder newString = new();
                newString.Append(word.InputBuffer.ToString().Substring(0, word.InputBuffer.Length - 1));

                word.InputBuffer = newString;
            }
        }
    }
    
    void Stroke(PloverStroke stroke)
    {
        paper.Add(stroke.Paper);
    }

    /// <summary>
    /// Get pixel width of word, including space
    /// </summary>
    /// <param name="word">The word to measure</param>
    /// <param name="truncate">only consider the number of characters in the input buffer</param>
    /// <returns>pixel width of word</returns>
    int GetWordWidth(Word word, bool truncate = false)
    {
        string text = word.Text;
        var inputBuffer = word.InputBuffer;
        if (inputBuffer.Length > word.Text.Length)
        {
            var extraInput = inputBuffer.ToString().Substring(word.Text.Length);
            text += extraInput;
        }

        if (truncate)
        {
            return Util.GetTextWidth(text.Substring(0, word.InputBuffer.Length), primaryFont);
        }
        return Util.GetTextWidth(text + " ", primaryFont);
    }

    int DrawWord(Word word, Vector2 pos, bool visited)
    {
        string text = word.Text;
        StringBuilder buffer = word.InputBuffer;

        DrawInputBuffer(word, pos);
        pos.Y += primaryFont.BaseSize;

        int totalWidth = 0;

        for (int i = 0; i < Math.Max(text.Length, buffer.Length); i++)
        {
            bool textContains = text.Length > i;
            bool bufferContains = buffer.Length > i;
            float fontSize = primaryFont.BaseSize;

            string str = "";
            Color color = Color.Black;

            // Change color/text depending on the valid/error state
            if (textContains && !bufferContains)
            {
                str = text[i].ToString();
                color = Shared.TextColor;
            }
            else if (bufferContains && !textContains)
            {
                str = buffer[i].ToString();
                color = Shared.AltErrTextColor;
            }
            else if (buffer[i] != text[i])
            {
                str = text[i].ToString();
                color = Shared.ErrTextColor;
            }
            else // Match
            {
                str = buffer[i].ToString();
                color = Shared.AltTextColor;
            }
            Util.DrawText(primaryFont, str, pos, color);

            int width = Util.GetTextWidth(str, primaryFont);
            pos.X += width;
            totalWidth += width;
        }

        // Error underline
        if (visited && word.Text != word.InputBuffer.ToString())
        {
            // Move cursor back to the start of the word, and down
            pos.X -= totalWidth;
            pos.Y += primaryFont.BaseSize;

            int height = (int)(primaryFont.BaseSize * .08f);
            DrawRectangle((int)pos.X, (int)pos.Y, totalWidth, height, Shared.ErrTextColor);
        }

        return totalWidth + Util.GetTextWidth(" ", primaryFont);
    }
    
    void DrawInputBuffer(Word word, Vector2 pos)
    {
        string inputBuffer = Util.RemoveControlCharacters(word.InputBuffer.ToString());
        Util.DrawText(primaryFont, inputBuffer, pos, Shared.AltTextColor);
    }
    
    string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);

        return $"{minutes,3:D1}:{seconds:D2}";
    }

    string FormatWpm(int wpm)
    {
        if (wpm > 999)
        {
            wpm = 999;
        }
        return $"{wpm,3} WPM";
    }
}