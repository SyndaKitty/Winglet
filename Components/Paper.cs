using Raylib_cs;
using System.Numerics;

public class Paper
{
    Font font;
    int charWidth;
    CircularBuffer<string> buffer;

    const int PaperChars = 23;
    const int PaperLines = 10;
    const int DefaultFontSize = 30;

    public int Width => charWidth * PaperChars;
    public int Height => font.BaseSize * PaperLines;

    public Paper()
    {
        buffer = new(PaperLines);
        Input.OnPaper += HandlePaper;

        SetFont(Shared.GetFont(Shared.PrimaryFontFile, DefaultFontSize));
    }

    public void SetFont(Font font)
    {
        this.font = font;
        charWidth = Util.GetTextWidth(" ", font);
    }

    public void Draw(Vector2 topLeft)
    {
        if (font.BaseSize == 0) return;

        Raylib.DrawRectangle((int)topLeft.X, (int)topLeft.Y, Width, Height, Shared.PanelColor);
        Vector2 cursor = topLeft;
        cursor.Y += Height - font.BaseSize;
        foreach (var str in buffer)
        {
            if (str is not null)
            {
                Util.DrawText(font, str, cursor, Shared.AltTextColor);
            }
            cursor.Y -= font.BaseSize;
        }
    }

    void HandlePaper(string paper)
    {
        buffer.Add(paper);
    }
}