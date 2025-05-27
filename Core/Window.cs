using static Raylib_cs.Raylib;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;

public static class Window
{
    const string Tag = "Window";

    static Scene? currentScene => sceneStack.Any() ? sceneStack.First() : null;
    static List<(Scene? scene, SceneChange type)> sceneActions = [];
    static Stack<Scene> sceneStack = [];

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

        if (settings.MSAA)
        {
            SetWindowState(ConfigFlags.Msaa4xHint);
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
            if (sceneActions.Count > 0)
            {
                ProcessSceneActions();
            }

            var scene = currentScene;
            currentScene?.Update();

            BeginDrawing();
            rlImGui.Begin();
            
            scene?.Draw();
            
            rlImGui.End();
            EndDrawing();

            Log.DispatchMessages();
        }

        Log.Info(Tag, "Beginning shutdown");
        rlImGui.Shutdown();
        CloseWindow();
    }

    public static void PushScene(Scene nextScene)
    {
        sceneActions.Add((nextScene, SceneChange.Push));
    }

    public static void PopScene()
    {
        sceneActions.Add((null, SceneChange.Pop));
    }

    public static void SetScene(Scene nextScene)
    {
        sceneActions.Add((nextScene, SceneChange.Set));
    }

    static void ProcessSceneActions()
    {
        foreach (var action in sceneActions)
        {
            if (currentScene != null)
            {
                Log.Trace(Tag, $"Unloading scene: {currentScene}");
                currentScene.Unload();
            }

            if (action.type == SceneChange.Set)
            {
                sceneStack.Clear();
                sceneStack.Push(action.scene!);
            }
            else if (action.type == SceneChange.Push)
            {
                sceneStack.Push(action.scene!);
            }
            else if (action.type == SceneChange.Pop)
            {
                sceneStack.Pop();
            }

            Log.Trace(Tag, $"Loading scene: {currentScene}");
            currentScene?.Load();
        }
        sceneActions.Clear();
    }

    enum SceneChange
    {
        Push,
        Pop,
        Set
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
    public bool MSAA;
}
