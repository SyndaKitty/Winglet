using Raylib_cs;
using System.Numerics;
using System.Text;
using static Raylib_cs.Raylib;

public class PracticeScene : Scene
{
    const string Tag = "PracticeScene";

    CourseLesson lesson;
    PloverServer server;
    WPM wpm;
    Paper paper;

    // Words
    string targetText;
    //List<(int start, int length)> targetWordBoundaries;
    List<Word> words;
    int currentWordIndex;

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

    // Console
    DebugConsole console;

    public PracticeScene(CourseLesson lesson, PloverServer server, DebugConsole console, Paper paper)
    {
        this.lesson = lesson;
        this.server = server;
        this.paper = paper;
        this.console = console;
        wpm = new();
        words = new();
        //recordedInput = new();

        targetText = lesson.Prompts ?? "";
        // Do something else for strict spaces
        var targetWords = targetText.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        words = targetWords.Select(w => new Word {
            Target = w
        }).ToList();

        Input.OnTextTyped += OnTextTyped;
        Input.OnBackspace += OnBackspace;
    }

    public void Load() 
    {
        SetFontSize(40);
    }

    public void Unload() { }
    
    public void Draw() 
    {
        const float padding = 10f;

        ClearBackground(Shared.BackgroundColor);

        int leftPanelWidth = Math.Max(primaryCharWidth * 8 + (int)padding * 2, paper.Width);
        int mainPanelWidth = GetScreenWidth() - leftPanelWidth * 2;

        DrawWords(leftPanelWidth, mainPanelWidth);
        
        Vector2 origin = new Vector2(leftPanelWidth + padding, GetScreenHeight() / 2 - primaryFont.BaseSize);
        Vector2 cursor = origin;

        // Draw left panel
        DrawRectangle(0, 0, leftPanelWidth, GetScreenHeight() + 1, Shared.PanelColor);
        
        // Draw paper
        cursor.X = 0;
        cursor.Y = 0;
        paper.Draw(cursor);

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

        console.Draw();
    }

    void DrawWords(int leftMost, int width)
    {
        const int vPadding = 10;
        const int hPadding = 40;
        int rightMost = leftMost + width - hPadding;
        leftMost += hPadding;

        Vector2 cursor = new Vector2(leftMost, GetScreenHeight() / 2);
        foreach (var word in words)
        {
            string inputWord = word.InputBuffer.ToString().Trim();
            int targetWidth = Util.GetTextWidth(" " + word.Target, primaryFont);
            int inputWidth = Util.GetTextWidth(" " + inputWord, primaryFont);
            int textWidth = Math.Max(targetWidth, inputWidth);

            if (cursor.X + textWidth > rightMost - hPadding)
            {
                cursor.Y += (primaryFont.BaseSize + vPadding) * 2;
                cursor.X = leftMost;
            }

            Util.DrawText(primaryFont, inputWord, cursor, Shared.AltTextColor);
            cursor.Y += primaryFont.BaseSize;
            Util.DrawText(primaryFont, word.Target, cursor, Shared.TextColor);
            
            cursor.X += textWidth;
            cursor.Y -= primaryFont.BaseSize;
        }
    }

    void TextChanged()
    {
        var word = words[currentWordIndex];
        var inputWord = word.InputBuffer.ToString();
        if (inputWord.Trim() == word.Target)
        {
            currentWordIndex++;
            Log.Trace(Tag, $"Now on word {currentWordIndex}: {words[currentWordIndex].Target}");
        }
        else if (currentWordIndex < words.Count - 1)
        {
            // add back trimmed spaces
            inputWord = word.InputBuffer.ToString();
            var inputWords = inputWord.Split(" ");
            var nextWord = words[currentWordIndex + 1].Target;
            for (int i = 1; i < inputWords.Length; i++)
            {
                if (inputWords[i] == nextWord)
                {
                    // Split the input into two, where the second part contains the new word
                    int splitPoint = inputWord.IndexOfInstance(' ', i);

                    string prevPart = inputWord.Substring(0, splitPoint);
                    string nextPart = inputWord.Substring(splitPoint);

                    Log.Trace(Tag, $"Found valid next word, splitting input buffer. prevPart: \"{prevPart}\" nextPart: \"{nextPart}\"");
                    word.InputBuffer = new(prevPart);

                    currentWordIndex++;
                    words[currentWordIndex].InputBuffer = new(nextPart);

                    TextChanged();
                    break;
                }
            }
        }
    }

    void OnBackspace(int count)
    {
        var buffer = words[currentWordIndex].InputBuffer;
        while (count > 0)
        {
            if (buffer.Length < count)
            {
                count -= buffer.Length;
                buffer.Length = 0;

                if (currentWordIndex == 0)
                {
                    return;
                }

                currentWordIndex -= 1;
                buffer = words[currentWordIndex].InputBuffer;
            }
            else
            {
                buffer.Length -= count;
                count = 0;
            }
        }

        TextChanged();
    }

    void OnTextTyped(string text)
    {
        var word = words[currentWordIndex];
        word.Add(text);
        
        TextChanged();
    }

    void SetFontSize(int fontSize)
    {
        primaryFont = Shared.GetFont(Shared.PrimaryFontFile, fontSize);
        secondaryFont = Shared.GetFont(Shared.SecondaryFontFile, fontSize);
        smallFont = Shared.GetFont(Shared.PrimaryFontFile, (int)(fontSize * .5f));

        // Assuming mono-spaced font, so character width will be consistent
        primaryCharWidth = Util.GetTextWidth(" ", primaryFont);

        paper.SetFont(smallFont);
    }

    List<(int start, int length)> CalculateWordBoundaries(StringBuilder buffer)
    {
        List<(int start, int length)> boundaries = [];

        bool inWord = false;
        int wordStart = -1;
        for (int i = 0; i < buffer.Length; i++)
        {
            if (!inWord && buffer[i] != ' ')
            {
                inWord = true;
                wordStart = i;
            }
            else if (inWord && buffer[i] == ' ')
            {
                inWord = false;
                boundaries.Add((wordStart, i - wordStart));
            }
        }
        if (inWord)
        {
           boundaries.Add((wordStart, buffer.Length - wordStart));
        }

        return boundaries;
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

    class Word
    {
        public string Target = "";
        public StringBuilder InputBuffer = new();

        public void Add(string text)
        {
            InputBuffer.Append(text);
        }

        public bool Backspace(int count)
        {
            if (count > InputBuffer.Length)
            {
                InputBuffer.Length = 0;
                return false;
            }
            
            InputBuffer.Length -= count;
            return true;
        }
    }
}