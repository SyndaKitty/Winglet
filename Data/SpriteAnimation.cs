using Raylib_cs;
using System.Diagnostics;
using System.Numerics;

public class SpriteAnimation
{
    SpriteSheet? sheet;
    float fps;
    float t;
    int frameCount;
    string? filePath;

    public float Width => sheet != null ? sheet.Value.Texture.Width / sheet.Value.FrameCount : 0;
    public float Height => sheet != null ? sheet.Value.Texture.Height : 0;

    public SpriteAnimation(string filePath, int frameCount, float fps)
    {
        this.fps = fps;
        this.filePath = filePath;
        this.frameCount = frameCount;
    }

    public SpriteAnimation(SpriteSheet sheet, float fps)
    {
        this.sheet = sheet;
        this.fps = fps;
    }

    public void Load()
    {
        if (sheet == null)
        {
            Debug.Assert(filePath != null, "Somehow filepath was never set");
            sheet = Shared.LoadSpriteSheet(filePath, frameCount);
        }
    }

    public void Update()
    {
        t += Raylib.GetFrameTime() * fps;
    }

    public void Draw(Vector2 pos, Color tint)
    {
        int frameIndex = Util.Mod((int)t, sheet!.Value.FrameCount);

        var frame = sheet!.Value.GetFrame(frameIndex);
        Raylib.DrawTextureRec(sheet!.Value.Texture, frame, pos, tint);
    }

    public void Draw(float x, float y, Color tint)
    {
        Draw(new(x, y), tint);
    }
}