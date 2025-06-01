using Raylib_cs;
using System.Numerics;

public class CourseSelection : Scene
{
    const string CoursePath = "Resources/Courses";
    
    public const string Tag = "CourseSelection";

    Font primaryFont;
    List<Course> courses = [];
    int selectedIndex = 0;

    Color standardColor = Shared.TextColor;
    Color selectedColor = Shared.AltTextColor;

    float yOffset;
    int lineGap => primaryFont.BaseSize + 20;

    PloverServer server;
    Paper paper;
    DebugConsole console;
    KeyboardDisplay keyboard;
    
    // Scores
    ScoreGraph? graph;
    List<LessonResult> scores;

    bool expectNewline;
    bool gotNewline;

    public CourseSelection(PloverServer server, DebugConsole? console, Paper? paper, KeyboardDisplay? keyboard)
    {
        this.server = server;
        this.paper = paper ?? new();
        this.console = console ?? new();
        this.keyboard = keyboard ?? new KeyboardDisplay();
        scores = [];
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

        Log.Info(Tag, "Loading score records");
        scores = LessonResult.Deserialize(File.ReadAllText(Shared.ResultFilePath));

        if (scores.Count > 0)
        {
            var lastCourseDone = courses.FirstOrDefault(x => scores.First().LessonName == x.Lessons[0].Name);
            if (lastCourseDone != null)
            {
                selectedIndex = courses.IndexOf(lastCourseDone);
            }
        }
        SelectNewCourse();

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

        int x = Raylib.GetScreenWidth() / 2;
        int y = Raylib.GetScreenHeight() / 2;
        graph?.Draw(new(x, y), new(x, y));
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
                SelectNewCourse();
            }
        }

        if (key == "W-" || key == "-B")
        {
            int prevIndex = selectedIndex;
            selectedIndex = Math.Min(courses.Count - 1, selectedIndex + 1);
            if (prevIndex != selectedIndex)
            {
                yOffset -= lineGap;
                SelectNewCourse();
            }
        }
    }

    void SelectNewCourse()
    {
        graph = new ScoreGraph(scores, courses[selectedIndex].Lessons[0]);
        graph.Load();
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