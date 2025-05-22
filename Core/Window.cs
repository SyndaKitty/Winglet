using static Raylib_cs.Raylib;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;

public static class Window
{
    const string Tag = "Window";

    static Scene? currentScene;
    static Scene? nextScene;

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
            var fontPtr = ptr.Fonts.AddFontFromFileTTF("Resources/Hack-Regular.ttf", 14);
            unsafe
            {
                ptr.NativePtr->FontDefault = fontPtr;
            }
        };

        rlImGui.Setup(true);
        //ImGuiTheme.SetupImGuiStyle();

        SetScene(startScene);

        while (!WindowShouldClose())
        {
            if (nextScene != null)
            {
                LoadScene(nextScene);
                nextScene = null;
            }

            currentScene?.Update();

            BeginDrawing();
            rlImGui.Begin();
            
            currentScene?.Draw();
            
            rlImGui.End();
            EndDrawing();

            Log.DispatchMessages();
        }

        Log.Info(Tag, "Beginning shutdown");
        rlImGui.Shutdown();
        CloseWindow();
    }

    public static void SetScene(Scene nextScene)
    {
        Window.nextScene = nextScene;
    }

    static void LoadScene(Scene newScene)
    {
        if (currentScene != null)
        {
            currentScene.Unload();
        }

        currentScene = newScene;
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
