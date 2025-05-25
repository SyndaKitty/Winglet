
using Raylib_cs;
using System.Numerics;

public class KeyboardDisplay
{
    HashSet<string> lastPressed;
    List<List<(string input, string display)>> Layout = [
        [("#","#"), ("T-", "T"), ("P-", "P"), ("H-", "H") ],
        [("-F", "F"), ("-P", "P"), ("-L", "L"), ("-T", "T"), ("-D", "D") ],
        [("S-","S"), ("K-", "K"), ("W-", "W"), ("R-", "R") ],
        [("-R", "R"), ("-B", "B"), ("-G", "G"), ("-S", "S"), ("-Z", "Z") ],
        [("A-", "A"), ("O-", "O")],
        [("-E", "E"), ("-U", "U")],
        [("*", "*")],
    ];
    Font font;

    public KeyboardDisplay()
    {
        Input.OnStenoKeys += HandleKeys;
        lastPressed = new();

        font = Shared.GetFont(Shared.PrimaryFontFile, 20);
    }

    ~KeyboardDisplay()
    {
        Input.OnStenoKeys -= HandleKeys;
    }

    public void SetFont(Font font)
    {
        this.font = font;
    }

    void HandleKeys(List<string> keys)
    {
        lastPressed = keys.ToHashSet();


    }

    public void Draw(Vector2 pos, float width)
    {
        // All ratios relative to standard key width
        const float StandardKeyHeightRatio = 1.2f;
        const float HorizontalGapRatio = .1f;
        const float VerticalGapRatio = .1f;
        const float AltKeyWidthRatio = 1.2f;
        const float VowelHorizontalGapRatio = .4f;

        float Round(float x) => x;// MathF.Round(x);

        float keyWidth = Round(width / (9 + 9 * HorizontalGapRatio + AltKeyWidthRatio));
        float keyHeight = Round(keyWidth * StandardKeyHeightRatio);

        float horizontalGap = Round(keyWidth * HorizontalGapRatio);
        float verticalGap = Round(keyHeight * VerticalGapRatio);

        float altKeyWidth = Round(keyWidth * AltKeyWidthRatio);
        float altKeyHeight = Round(keyHeight * 2 + verticalGap);

        float vowelGap = Round(keyWidth * VowelHorizontalGapRatio);

        Vector2 startPos = pos;
        Vector2 standardKeySize = new Vector2(keyWidth, keyHeight);

        // Adjust font size to keys
        int newFontSize = (int)Math.Round(keyHeight * .75f);
        if (font.BaseSize != newFontSize)
        {
            SetFont(Shared.GetFont(Shared.PrimaryFontFile, newFontSize));
        }

        // Top left
        DrawKeys(Layout[0], pos, standardKeySize, horizontalGap);

        // Top Right
        pos.X = startPos.X + (keyWidth + horizontalGap) * 4 + altKeyWidth + horizontalGap;
        DrawKeys(Layout[1], pos, standardKeySize, horizontalGap);
        
        // Bottom left
        pos.X = startPos.X;
        pos.Y += keyHeight + verticalGap;
        DrawKeys(Layout[2], pos, standardKeySize, horizontalGap);

        // Bottom right
        pos.X = startPos.X + (keyWidth + horizontalGap) * 4 + altKeyWidth + horizontalGap;
        DrawKeys(Layout[3], pos, standardKeySize, horizontalGap);

        // Left vowels
        float middle = (keyWidth + horizontalGap) * 4 + altKeyWidth / 2;
        pos.X = startPos.X + middle - vowelGap / 2 - keyWidth * 2 - horizontalGap;
        pos.Y = startPos.Y + (keyHeight + verticalGap) * 2;
        DrawKeys(Layout[4], pos, standardKeySize, horizontalGap);

        // Right vowels
        pos.X = startPos.X + middle + vowelGap / 2;
        DrawKeys(Layout[5], pos, standardKeySize, horizontalGap);

        // Alt
        pos.X = startPos.X + (keyWidth + horizontalGap) * 4;
        pos.Y = startPos.Y;
        DrawKeys(Layout[6], pos, new(altKeyWidth, altKeyHeight), horizontalGap);

    }

    void DrawKeys(List<(string input, string display)> keys, Vector2 pos, Vector2 size, float gap)
    {
        //const float Roundness = .15f;
        const float lineThickness = 2f;
        Vector2 letterOffset = new(size.X - Util.GetTextWidth(" ", font), size.Y - font.BaseSize);
        letterOffset *= .5f;
        for (int i = 0; i < keys.Count; i++)
        {
            bool pressed = lastPressed.Contains(keys[i].input);

            var rect = new Rectangle {
                X = pos.X,
                Y = pos.Y,
                Width = size.X,
                Height = size.Y
            };

            // back
            Color backingColor = Shared.PanelColor;
            if (pressed) backingColor = Shared.AccentColor;
            Util.DrawRectangle(pos, size, backingColor);

            // letter
            Color letterColor = Shared.AccentColor;
            if (pressed) letterColor = Shared.PanelColor;
            Util.DrawText(font, keys[i].display, pos + letterOffset, letterColor);
            
            // border
            Raylib.DrawRectangleLinesEx(rect, lineThickness, Shared.AccentColor);
            
            //Raylib.DrawRectangleRoundedLinesEx(rect, Roundness, 100, lineThickness, Shared.AccentColor);
            //Util.DrawRectangle(pos, size, Shared.AccentColor, Roundness);

            pos.X += size.X + gap;
        }
    }
}