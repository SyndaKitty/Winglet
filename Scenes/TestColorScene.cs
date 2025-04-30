using System.Numerics;
using static Raylib_cs.Raylib;
using Raylib_cs;

public class TestColorScene: Scene
{
    Color backgroundColor = new Color(51, 58, 69);
    Color panelColor = new Color(44, 49, 59);
    Color accentColor = new Color(244, 76, 127);
    Color textColor = new Color(147, 158, 174);

    Font primaryFont;

    public void Load() 
    {
        primaryFont = Shared.GetFont(Shared.PrimaryFontFile, 30);
    }

    public void Unload() { }
    public void Update() { }

    public void Draw()
    {
        ClearBackground(backgroundColor);
        DrawRectangle(40, 40, 370, 50, panelColor);
        DrawTextEx(primaryFont, "Testing", new Vector2(50, 50), 30, 0, textColor);
        DrawTextEx(primaryFont, "special", new Vector2(170, 50), 30, 0, accentColor);
        DrawTextEx(primaryFont, "words", new Vector2(300, 50), 30, 0, textColor);
    }
}