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
});

// Render a single frame to get to blank screen ASAP
Raylib.BeginDrawing();
Raylib.ClearBackground(Shared.BackgroundColor);
Raylib.EndDrawing();

PloverServer server = new();
Input.SetServer(server);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
{
    Task.Run(server.Connect);
}
#pragma warning restore CS4014

Window.Run(new CourseSelection(server));
//Window.Run(new PracticeScene(Course.Load("Resources/Courses/00_Introductions.yaml")?.Lessons[0]));

Log.Stop();