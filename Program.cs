const int screenWidth = 1200;
const int screenHeight = 800;
const int targetFPS = 144;

Window.Create(new WindowSettings {
    Width = screenWidth,
    Height = screenHeight,
    Title = "Wingman",
    TargetFPS = targetFPS
});

Window.Run(new PracticeScene("Resources/Lessons/lesson01.txt"));