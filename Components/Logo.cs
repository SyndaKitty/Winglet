using Raylib_cs;
using System.Numerics;

public class Logo
{

    string[] letterImageNames = [ "W", "I", "N", "G", "L", "E", "T" ];
    string[] wingImageNames = ["Wing1", "Wing2", "Wing3"];
    
    List<Texture2D> letters;
    List<Texture2D> wings;
    float t;

    Shader logoShader;
    int texLocation;
    int tLocation;
    int colorLocation;

    public int Width => letters[0].Width;
    public int Height => letters[0].Height;

    public Logo()
    {
        letters = [];
        wings = [];
    }

    public void Load()
    {
        foreach (var image in letterImageNames)
        {
            letters.Add(Shared.LoadTexture($"Logo{image}.png"));
        }
        foreach (var image in wingImageNames)
        {
            wings.Add(Shared.LoadTexture($"Logo{image}.png"));
        }

        logoShader = Raylib.LoadShader(null, "Resources/Shaders/fadein.glsl");
        texLocation = Raylib.GetShaderLocation(logoShader, "texture0");
        tLocation = Raylib.GetShaderLocation(logoShader, "t");
        colorLocation = Raylib.GetShaderLocation(logoShader, "aaaaa");
    }

    public void Unload()
    {
        Raylib.UnloadShader(logoShader);
    }

    public void Update()
    {
        t += Raylib.GetFrameTime();

        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            t = 0;
        }
    }

    public void Draw(Vector2 pos)
    {
        //Raylib.BeginBlendMode(BlendMode.Alpha);

        // Wing/base
        for (int i = 0; i < wings.Count; i++)
        {
            var subImage = wings[i];
            Raylib.BeginShaderMode(logoShader);
            Raylib.SetShaderValue(logoShader, tLocation, t, ShaderUniformDataType.Float);
            Raylib.SetShaderValueTexture(logoShader, texLocation, subImage);
            Raylib.SetShaderValue(logoShader, colorLocation, Util.ToVector(Shared.TransColors[i]), ShaderUniformDataType.Vec3);
            Raylib.DrawTexture(subImage, (int)pos.X, (int)pos.Y, Shared.TransColors[i]);
            Raylib.EndShaderMode();
        }


        // Letters
        for (int i = 0; i < letters.Count; i++)
        {
            float subT = t - i * .05f - 1f;
            var subImage = letters[i];

            var white = Color.White;
            var rainbow = Shared.RainbowColors[i];
            var brightRainbow = Util.LerpColor(rainbow, Color.White, .4f);


            float l = .5f;
            float curve = -4f / l / l * (subT - l / 2f) * (subT - l / 2f) + 1;
            curve = Math.Max(curve, 0);
            
            float colorT = Math.Max(subT > .4f ? .5f : 0f, curve);
            Color c;
            if (colorT < .5f)
            {
                c = Util.LerpColor(white, rainbow, colorT / .5f);
            }
            else
            {
                c = Util.LerpColor(rainbow, brightRainbow, (colorT - .5f) / .5f);
            }

            Raylib.BeginShaderMode(logoShader);
            Raylib.SetShaderValue(logoShader, tLocation, t, ShaderUniformDataType.Float);
            Raylib.SetShaderValueTexture(logoShader, texLocation, subImage);
            Raylib.SetShaderValue(logoShader, colorLocation, Util.ToVector(c), ShaderUniformDataType.Vec3);
            Raylib.DrawTexture(subImage, (int)pos.X, (int)(pos.Y - curve * 40f), c);
            Raylib.EndShaderMode();
        }

        //Raylib.EndBlendMode();
    }
}