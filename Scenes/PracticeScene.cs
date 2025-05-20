using Raylib_cs;
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

    CourseLesson lesson;
    List<List<Word>> lines;
    PloverServer server;
    Input input;
    WPM wpm;
    List<string> paper;
    Stack<string> rawInputBuffer;

    bool complete = false;
    float timeSinceComplete = 0f;

    int lineIndex = 0;
    int wordIndex = 0;

    // Used for smooth scrolling
    List<float> xOffset;
    float yOffset;
    
    float smoothProgressPercent = 0;
    int totalWordCount = 0;

    bool timerRunning = false;
    float timer = 0;
    float timeSinceType = 0;

    const float StopTimingThreshold = 5f;

    int lineGap => primaryFont.BaseSize * 2 + 40;

    public PracticeScene(CourseLesson lesson)
    {
        this.lesson = lesson;
        lines = new();
        server = new();
        input = new(server);
        wpm = new();
        paper = new();
        rawInputBuffer = new();
        xOffset = [];

        input.OnBackspace += Backspace;
        input.OnTextTyped += TextTyped;

        server.OnSendStroke += Stroke;

        SetFontSize(60);

    }

    public void Load() 
    {
        lines = lesson.GetWords() ?? [];
        totalWordCount = lines.Sum(x => x.Count);
        xOffset.AddRange(lines.Select(x => 0f));

        try
        {
            Task.Run(() => {
                bool connected = server.Connect().GetAwaiter().GetResult();
                if (connected)
                {
                    input.SetInputMode(Input.Mode.Plover);
                }
                else
                {
                    input.SetInputMode(Input.Mode.Keyboard);
                }
            });
        }
        catch
        {

        }

    }

    public void Unload() { }
    
    public void Draw() 
    {
        const float padding = 10f;

        ClearBackground(Shared.BackgroundColor);

        Vector2 origin = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2 - primaryFont.BaseSize);
        Vector2 cursor = origin;

        for (int i = 0; i < lines.Count; i++)
        {
            DrawWordLine(i, cursor, padding);
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
        int width = primaryCharWidth * 8 + (int)padding * 2;
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
        float progressPercent = (lines.Take(lineIndex).Sum(x => x.Count) + wordIndex) / (float)totalWordCount;
        if (complete)
        {
            // If we are done, set progress to 100%
            progressPercent = 1;
        }
        smoothProgressPercent = Util.ExpDecay(smoothProgressPercent, progressPercent, Shared.SlideSpeed, GetFrameTime());
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

        for (int i = 0; i < lines.Count; i++)
        {
            xOffset[i] = Util.ExpDecay(xOffset[i], 0, Shared.SlideSpeed, GetFrameTime());
        }
        yOffset = Util.ExpDecay(yOffset, 0, Shared.SlideSpeed, GetFrameTime());

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
    
    void DrawWordLine(int lineIndex, Vector2 cursor, float lineSpacing)
    {
        int wordIndex = this.wordIndex;
        if (this.lineIndex < lineIndex)
        {
            wordIndex = 0;
        }
        else if (this.lineIndex > lineIndex)
        {
            wordIndex = lines[lineIndex].Count - 1;
        }

        float startX = cursor.X;
        cursor.Y = GetScreenHeight() / 2 + lineGap * (lineIndex - this.lineIndex);
        cursor.Y += yOffset;
        bool activeLine = lineIndex == this.lineIndex;

        if (activeLine)
        {
            // Draw cursor
            int w = Math.Min(wordIndex, lines[lineIndex].Count - 1);
            Word word = lines[lineIndex][w];
            DrawRectangle((int)cursor.X + GetWordWidth(word, true), (int)cursor.Y, 2, primaryFont.BaseSize, Shared.AccentColor);
        }

        // Draw next words
        cursor.X += xOffset[lineIndex];
        for (int i = wordIndex; i < lines[lineIndex].Count; i++)
        {
            Word word = lines[lineIndex][i];
            cursor.X += DrawWord(word, cursor, false, activeLine);

            if (cursor.X > GetScreenWidth()) break;
        }

        // Draw visited words
        cursor.X = startX + xOffset[lineIndex];
        for (int i = wordIndex - 1; i >= 0 && i < lines[lineIndex].Count; i--)
        {
            Word word = lines[lineIndex][i];
            cursor.X -= GetWordWidth(word);
            DrawWord(word, cursor, true, activeLine);

            if (cursor.X < 0) break;
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
        if (lesson.Type != LessonType.Words) return;

        timerRunning = true;
        timeSinceType = 0;

        foreach (var key in text)
        {
            KeyTyped(key);
        }
    }

    void KeyTyped(char key)
    {
        if (complete) return;
        Word word = lines[lineIndex][wordIndex];

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

                if (Shared.UserSettings.SmoothScroll && wordIndex < lines[lineIndex].Count - 1)
                {
                    xOffset[lineIndex] += GetWordWidth(word, false);
                }

                wordIndex = Math.Min(wordIndex + 1, lines[lineIndex].Count - 1);
            }
        }
        else
        {
            word.InputBuffer.Append(key);

            // Check if we are done with this line
            if (wordIndex == lines[lineIndex].Count - 1)
            {
                // For word lessons, only complete the line if the word is correct
                if (word.InputBuffer.ToString() == word.Text)
                {
                    AdvanceLine();
                }
                // For raw input lessons, move onto the next line if any key was pressed on the last word
                else if (lesson.Type == LessonType.Raw && lines[lineIndex][wordIndex].InputBuffer.Length > 0)
                {
                    AdvanceLine();
                }
            }
        }
    }

    void AdvanceLine()
    {
        lineIndex++;
        if (lineIndex >= lines.Count)
        {
            complete = true;
            timerRunning = false;
            return;
        }
        wordIndex = 0;

        yOffset += lineGap;
    }

    void Backspace(int count)
    {
        if (complete) return;
        if (lesson.Type != LessonType.Words) return;

        // Stupid hack for skipping space on first word
        if (wordIndex == 0 && lines[lineIndex][wordIndex].InputBuffer.Length > 0 && count > 1) count--;
        
        for (int i = 0; i < count; i++)
        {
            var word = lines[lineIndex][wordIndex];
            if (word.InputBuffer.Length == 0)
            {
                PreviousWord();
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
    
    void PreviousWord()
    {
        if (wordIndex > 0)
        {
            wordIndex--;
            if (Shared.UserSettings.SmoothScroll)
            {
                xOffset[lineIndex] -= GetWordWidth(lines[lineIndex][wordIndex], false);
            }
        }
        else if (lineIndex > 0)
        {
            lineIndex--;
            wordIndex = lines[lineIndex].Count - 1;
            if (Shared.UserSettings.SmoothScroll)
            {
                yOffset -= lineGap;
            }
        }
    }

    void Stroke(PloverStroke stroke)
    {
        if (complete) return;
        paper.Add(stroke.Paper);

        if (lesson.Type != LessonType.Raw) return;
        
        if (stroke.Rtfcre == "*")
        {
            RawInputBackspace();
            return;
        }

        foreach (var c in stroke.Rtfcre)
        {
            KeyTyped(c);
        }
        KeyTyped(' ');
    }

    void RawInputBackspace()
    {
        PreviousWord();
        lines[lineIndex][wordIndex].InputBuffer.Clear();
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

    int DrawWord(Word word, Vector2 pos, bool visited, bool activeLine)
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

            if (!activeLine)
            {
                color = Shared.TextColor;
            }

            Util.DrawText(primaryFont, str, pos, color);

            int width = Util.GetTextWidth(str, primaryFont);
            pos.X += width;
            totalWidth += width;
        }

        // Error underline
        if (activeLine && visited && word.Text != word.InputBuffer.ToString())
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