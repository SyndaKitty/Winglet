using Raylib_cs;

public class IntroScene : Scene
{
    SpriteAnimation? limes;
    float t;
    Image logo;
    Texture2D logoTex;
    Shader logoShader;
    int texLocation;
    int tLocation;
    Scene nextScene;

    public IntroScene(Scene nextScene)
    {
        this.nextScene = nextScene;
    }

    public void Load()
    {
        limes = new SpriteAnimation("limesdance.png", 35, 24);

        logo = Shared.LoadImage("Logo.png");
        logoTex = Raylib.LoadTextureFromImage(logo);
        logoShader = Raylib.LoadShader(null, "Resources/Shaders/fadein.glsl");
        texLocation = Raylib.GetShaderLocation(logoShader, "texture0");
        tLocation = Raylib.GetShaderLocation(logoShader, "t");

        Raylib.SetTextureFilter(logoTex, TextureFilter.Trilinear);
    }
    
    public void Unload() 
    {
        Raylib.UnloadShader(logoShader);
    }

    public void Update()
    {
        t += Raylib.GetFrameTime() * 1.5f;

        if (t > 3.5f)
        {
            Window.SetScene(nextScene);
        }

        limes?.Update();
    }
    
    public void Draw()
    {
        float w = Raylib.GetScreenWidth();
        float h = Raylib.GetScreenHeight();
        float x = (w - logoTex.Width) *.5f;
        float y = (h - logoTex.Height) * .5f;

        Raylib.ClearBackground(Shared.BackgroundColor);
        Raylib.BeginBlendMode(BlendMode.Alpha);
        Raylib.BeginShaderMode(logoShader);
        {
            Raylib.SetShaderValueTexture(logoShader, texLocation, logoTex);
            Raylib.SetShaderValue(logoShader, tLocation, t, ShaderUniformDataType.Float);
            Raylib.DrawTexture(logoTex, (int)x, (int)y, Color.White);
        }
        Raylib.EndShaderMode();
        Raylib.EndBlendMode();

        if (limes != null)
        {
            x = Raylib.GetScreenWidth() - limes.Width;
            y = Raylib.GetScreenHeight() - limes.Height;
            limes?.Draw(x, y, Color.White);
        }
    }
}