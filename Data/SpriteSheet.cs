using Raylib_cs;

public struct SpriteSheet 
{
    public int FrameCount;
    public Texture2D Texture;

    public Rectangle GetFrame(int index)
    {
        int frameWidth = Texture.Width / FrameCount;
        int frameHeight = Texture.Height;
        return new Rectangle(index * frameWidth, 0, frameWidth, frameHeight);
    }
}