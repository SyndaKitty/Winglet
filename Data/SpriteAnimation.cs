using Raylib_cs;
using System.Numerics;

public class SpriteAnimation
{
    SpriteSheet sheet;
    float fps;
    float t;
    int frameCount;

    public float Width => sheet.Texture.Width / sheet.FrameCount;
    public float Height => sheet.Texture.Height;

    public SpriteAnimation(string filePath, int frameCount, float fps)
    {
        sheet = Shared.LoadSpriteSheet(filePath, frameCount);
        this.fps = fps;
    }

    public SpriteAnimation(SpriteSheet sheet, float fps)
    {
        this.sheet = sheet;
        this.fps = fps;
    }

    public void Update()
    {
        t += Raylib.GetFrameTime() * fps;
    }

    public void Draw(Vector2 pos, Color tint)
    {
        int frameIndex = Util.Mod((int)t, sheet.FrameCount);

        var frame = sheet.GetFrame(frameIndex);
        Raylib.DrawTextureRec(sheet.Texture, frame, pos, tint);
    }

    public void Draw(float x, float y, Color tint)
    {
        Draw(new(x, y), tint);
    }
}