using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

public class PracticeScene : Scene
{
    const string Tag = "PracticeScene";

    CourseLesson lesson;
    PloverServer server;
    WPM wpm;
    Paper paper;

    // Smooth scrolling
    float yOffset;
    
    // Progress bar
    float smoothProgressPercent = 0;
    int totalWordCount = 0;

    // Fonts
    Font primaryFont;
    Font secondaryFont;
    Font smallFont;
    int primaryCharWidth = 0;
    
    // Timer
    bool timerRunning = false;
    float timer = 0;
    float timeSinceType = 0;
    const float StopTimingThreshold = 5f;

    public PracticeScene(CourseLesson lesson, PloverServer server, Paper paper)
    {
        this.lesson = lesson;
        this.server = server;
        this.paper = paper;
        wpm = new();
    }

    public void Load() 
    {
        SetFontSize(60);
    }

    public void Unload() { }
    
    public void Draw() 
    {
        const float padding = 10f;

        ClearBackground(Shared.BackgroundColor);

        Vector2 origin = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2 - primaryFont.BaseSize);
        Vector2 cursor = origin;

        // TODO: draw words

        // Draw paper
        cursor.X = GetScreenWidth() - paper.Width;
        cursor.Y = 0;
        paper.Draw(cursor);

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
        //float progressPercent = (lines.Take(lineIndex).Sum(x => x.Count) + wordIndex) / (float)totalWordCount;
        float progressPercent = 0;
        smoothProgressPercent = Util.ExpDecay(smoothProgressPercent, progressPercent, Shared.SlideSpeed, GetFrameTime());
        int progressPixels = (int)(GetScreenWidth() * smoothProgressPercent);
        DrawRectangle(0, GetScreenHeight() - progressBarHeight, progressPixels, progressBarHeight  + 1, Shared.AltTextColor);
    }

    void SetFontSize(int fontSize)
    {
        primaryFont = Shared.GetFont(Shared.PrimaryFontFile, fontSize);
        secondaryFont = Shared.GetFont(Shared.SecondaryFontFile, fontSize);
        smallFont = Shared.GetFont(Shared.PrimaryFontFile, (int)(fontSize * .6f));

        // Assuming mono-spaced font, so character width will be consistent
        primaryCharWidth = Util.GetTextWidth(" ", primaryFont);

        paper.SetFont(smallFont);
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

    public void Update()
    {
        server.DispatchMessages();
    }
}