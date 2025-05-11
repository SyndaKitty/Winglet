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

Window.Run(new PracticeScene("Resources/Lessons/lesson01.txt"));