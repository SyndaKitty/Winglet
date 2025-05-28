using Raylib_cs;

const int screenWidth = 1200;
const int screenHeight = 800;
const int targetFPS = 144;

Window.Create(new WindowSettings {
    Width = screenWidth,
    Height = screenHeight,
    Title = "Wingman",
    TargetFPS = targetFPS,
    MinSize = (400, 300),
    Resizable = true,
    VSync = true,
    MSAA = true
});

// Render a single frame to get to blank screen ASAP
Raylib.BeginDrawing();
Raylib.ClearBackground(Shared.BackgroundColor);
Raylib.EndDrawing();

DebugConsole console = new();
PloverServer server = new();
Input.SetServer(server);

_ = Task.Run(server.Connect);

var courseSelection = new CourseSelection(server, console, null, null);
Window.Run(new IntroScene(courseSelection));
//Window.Run();
//Window.Run(new PracticeScene(Course.Load("Resources/Courses/00_Introductions.yaml")?.Lessons[0]));

Log.Stop();