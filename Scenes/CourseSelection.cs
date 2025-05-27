using Raylib_cs;
using System.Numerics;

public class CourseSelection : Scene
{
    const string CoursePath = "Resources/Courses";
    
    public const string Tag = "CourseSelection";

    Font primaryFont;
    List<Course> courses = [];
    int selectedIndex = 2;

    Color standardColor = Shared.TextColor;
    Color selectedColor = Shared.AltTextColor;

    float yOffset;
    int lineGap => primaryFont.BaseSize + 20;

    PloverServer server;
    Paper paper;
    DebugConsole console;
    KeyboardDisplay keyboard;

    bool expectNewline;
    bool gotNewline;

    public CourseSelection(PloverServer server, DebugConsole? console, Paper? paper, KeyboardDisplay? keyboard)
    {
        this.server = server;
        this.paper = paper ?? new();
        this.console = console ?? new();
        this.keyboard = keyboard ?? new KeyboardDisplay();
    }

    public void Load()
    {
        primaryFont = Shared.GetFont(Shared.PrimaryFontFile, 60);
        courses.Clear();

        var courseFiles = Directory.GetFiles(CoursePath, "*.yaml", SearchOption.TopDirectoryOnly)
            .OrderBy(x => x);
        
        foreach (var courseFile in courseFiles)
        {
            Course? c = Course.Load(courseFile);
            if (c != null)
            {
                courses.Add(c);
            }
        }

        Input.OnStenoKeys += HandleKeyInput;
        Input.OnTextTyped += HandleTextInput;
    }

    public void Unload() 
    {
        Input.OnStenoKeys -= HandleKeyInput;
        Input.OnTextTyped -= HandleTextInput;
    }

    public void Update()
    {
        server.DispatchMessages();
        yOffset = Util.ExpDecay(yOffset, 0, Shared.SlideSpeed, Raylib.GetFrameTime());

        if (expectNewline && !gotNewline)
        {
            Log.Warning(Tag, "Plover sent enter keys, but no newline received. Is output enabled?");
        }

        expectNewline = false;
        gotNewline = false;
    }
    
    public void Draw()
    {
        Raylib.ClearBackground(Shared.BackgroundColor);

        (int width, int height) = (Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        Vector2 cursor = new Vector2(0, 0);
        cursor.X = width * .25f;
        cursor.Y = height / 2 - primaryFont.BaseSize / 2;
        cursor.Y -= selectedIndex * lineGap + yOffset;
        
        for (int i = 0; i < courses.Count; i++)
        {
            var course = courses[i];
            Color color = (i == selectedIndex) ? selectedColor : standardColor;
            Util.DrawText(primaryFont, course.Name, cursor, color);
            cursor.Y += lineGap;
        }

        paper.Draw(new Vector2(width - paper.Width, 0));
        console.Draw();
        keyboard.Draw(new(width * .3f, 20f), width * .3f);
    }

    void KeyPress(string key)
    {
        if (key == "P-" || key == "-P")
        {
            int prevIndex = selectedIndex;
            selectedIndex = Math.Max(0, selectedIndex - 1);
            if (prevIndex != selectedIndex)
            {
                yOffset += lineGap;
            }
        }

        if (key == "W-" || key == "-B")
        {
            int prevIndex = selectedIndex;
            selectedIndex = Math.Min(courses.Count - 1, selectedIndex + 1);
            if (prevIndex != selectedIndex)
            {
                yOffset -= lineGap;
            }
        }
    }

    void HandleKeyInput(List<string> keys)
    {
        foreach (var key in keys)
        {
            KeyPress(key);
        }
        CheckForEnterKeys(keys);
    }

    void CheckForEnterKeys(List<string> keys)
    {
        if (keys.Count == 2)
        {
            var combo = (keys[0], keys[1]);
            if (combo == ("R-", "-R") || combo == ("-R", "R-"))
            {
                expectNewline = true;
            }
        }
    }

    void HandleTextInput(string text)
    {
        if (text == "\n")
        {
            gotNewline = true;
            Window.PushScene(new PracticeScene(courses[selectedIndex].Lessons[0], server, console, paper, keyboard));
        }
    }
}