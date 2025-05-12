using static Raylib_cs.Raylib;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;

public static class Window
{
    static Scene? currentScene;
    static bool sceneDirty;

    public static void Create(WindowSettings settings)
    {
        Log.Info("Window", $"Creating window {settings.Title ?? ""} ({settings.Width}x{settings.Height}) @ {settings.TargetFPS} Hz");
        InitWindow(settings.Width, settings.Height, settings.Title ?? "");
        SetWindowMinSize(settings.MinSize.Item1, settings.MinSize.Item2);
        SetTargetFPS(settings.TargetFPS ?? 60);
        if (settings.Resizable)
        {
            SetWindowState(ConfigFlags.ResizableWindow);
        }

        Shared.LoadUserSettings();

        if (settings.VSync)
        {
            SetWindowState(ConfigFlags.VSyncHint);
        }
        MaximizeWindow();
    }

    public static void Run(Scene startScene)
    {
        rlImGui.SetupUserFonts += (ImGuiIOPtr ptr) => {
            var fontPtr = ptr.Fonts.AddFontFromFileTTF("Resources/Hack-Regular.ttf", 24);
            unsafe
            {
                ptr.NativePtr->FontDefault = fontPtr;
            }

        };

        rlImGui.Setup(true);
        ImGuiTheme.SetupImGuiStyle();
        
        SetScene(startScene);

        while (!WindowShouldClose())
        {
            currentScene?.Update();
            
            // Avoid running draw before update when swapping scenes
            if (sceneDirty)
            {
                sceneDirty = false;
                continue;
            }

            BeginDrawing();
            rlImGui.Begin();
            
            currentScene?.Draw();
            
            rlImGui.End();
            EndDrawing();

        }

        rlImGui.Shutdown();
        CloseWindow();
    }

    public static void SetScene(Scene nextScene)
    {
        if (currentScene != null)
        {
            currentScene.Unload();
            sceneDirty = true;
        }

        currentScene = nextScene;
        currentScene.Load();
    }
}

public struct WindowSettings
{
    public int Width;
    public int Height;
    public int? TargetFPS;
    public string? Title;
    public (int, int) MinSize;
    public bool Resizable;
    public bool VSync;
}
