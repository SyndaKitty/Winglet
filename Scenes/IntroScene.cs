using Raylib_cs;

public class IntroScene : Scene
{
    SpriteAnimation limes;
    float t;
    Scene nextScene;
    Logo logo;
    PloverServer server;

    public IntroScene(Scene nextScene, PloverServer server)
    {
        this.nextScene = nextScene;
        this.server = server;
        logo = new();
        limes = new SpriteAnimation("limesdance.png", 35, 24);
    }

    public void Load()
    {
        limes.Load();
        logo.Load();
        Input.OnTextTyped += OnTextTyped;
        Input.OnStroke += OnStroke;
    }
    
    public void Unload() 
    {
        logo.Unload();
        Input.OnTextTyped -= OnTextTyped;
        Input.OnStroke -= OnStroke;
    }

    public void Update()
    {
        t += Raylib.GetFrameTime();

        if (t > 3f)
        {
            End();
        }

        logo.Update();
        limes.Update();
        server.DispatchMessages();
    }

    void End()
    {
        Window.SetScene(nextScene);
    }

    public void Draw()
    {
        Raylib.ClearBackground(Shared.BackgroundColor);
        
        float w = Raylib.GetScreenWidth();
        float h = Raylib.GetScreenHeight();
        float x = (w - logo.Width) *.5f;
        float y = (h - logo.Height) * .5f;
        logo.Draw(new(x, y));

        x = Raylib.GetScreenWidth() - limes.Width;
        y = Raylib.GetScreenHeight() - limes.Height;
        limes.Draw(x, y, Color.White);
    }

    void OnTextTyped(string text)
    {
        End();
    }

    void OnStroke(string stroke)
    {
        End();
    }
}