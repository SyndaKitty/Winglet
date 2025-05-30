using Raylib_cs;

public class IntroScene : Scene
{
    SpriteAnimation limes;
    float t;
    //Image logo;
    //Texture2D logoTex;
    //Shader logoShader;
    //int texLocation;
    //int tLocation;
    Scene nextScene;
    Logo logo;

    public IntroScene(Scene nextScene)
    {
        this.nextScene = nextScene;
        logo = new();
        limes = new SpriteAnimation("limesdance.png", 35, 24);
    }

    public void Load()
    {

        //logo = Shared.LoadImage("Logo.png");
        //logoTex = Raylib.LoadTextureFromImage(logo);
        //logoShader = Raylib.LoadShader(null, "Resources/Shaders/fadein.glsl");
        //texLocation = Raylib.GetShaderLocation(logoShader, "texture0");
        //tLocation = Raylib.GetShaderLocation(logoShader, "t");
        //Raylib.SetTextureFilter(logoTex, TextureFilter.Trilinear);

        limes.Load();
        logo.Load();
    }
    
    public void Unload() 
    {
        //Raylib.UnloadShader(logoShader);
    }

    public void Update()
    {
        t += Raylib.GetFrameTime() * 1.5f;

        if (t > 3.5f)
        {
            //Window.SetScene(nextScene);
        }

        logo.Update();
        limes.Update();
    }
    
    public void Draw()
    {
        Raylib.ClearBackground(Shared.BackgroundColor);
        
        float w = Raylib.GetScreenWidth();
        float h = Raylib.GetScreenHeight();
        float x = (w - logo.Width) *.5f;
        float y = (h - logo.Height) * .5f;
        logo.Draw(new(x, y));
        /*

        Raylib.BeginBlendMode(BlendMode.Alpha);
        Raylib.BeginShaderMode(logoShader);
        {
            Raylib.SetShaderValueTexture(logoShader, texLocation, logoTex);
            Raylib.SetShaderValue(logoShader, tLocation, t, ShaderUniformDataType.Float);
            Raylib.DrawTexture(logoTex, (int)x, (int)y, Color.White);
        }
        Raylib.EndShaderMode();
        Raylib.EndBlendMode();
        */

        x = Raylib.GetScreenWidth() - limes.Width;
        y = Raylib.GetScreenHeight() - limes.Height;
        limes.Draw(x, y, Color.White);
    }
}