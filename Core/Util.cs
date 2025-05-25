using Raylib_cs;
using System.Numerics;
using System.Text;

public static class Util
{
    public const string Tag = "Util";

    public static float ExpDecay(float a, float b, float decay, float dt)
    {
        return b + (a - b) * MathF.Exp(-decay * dt);
    }

    public static void DrawText(Font font, string text, Vector2 pos, Color color)
    {
        Raylib.DrawTextEx(font, text, pos, font.BaseSize, 0, color);
    }

    public static void DrawRectangle(Vector2 pos, Vector2 size, Color color)
    {
        Vector2Int posInt = new(pos);
        Vector2Int sizeInt = new(size);

        Raylib.DrawRectangle(posInt.X, posInt.Y, sizeInt.X, sizeInt.Y, color);
    }

    public static void DrawRectangle(Vector2 pos, Vector2 size, Color color, float roundness)
    {
        Raylib.DrawRectangleRounded(
            new Rectangle { 
                X = pos.X, Y = pos.Y, 
                Width = size.X, Height = size.Y 
            }, 
            roundness, 0, color
        );
    }

    public static unsafe int GetTextWidth(string text, Font font)
    {
        const int GetCodepointError = 0x3f;

        float width = 0;

        for (int i = 0; i < text.Length; i++)
        {
            int codepointByteCount = 0;
            int codepoint = Raylib.GetCodepoint(text[i].ToString(), ref codepointByteCount);
            if (codepoint == GetCodepointError)
            {
                Log.Error(Tag, $"Unable to get codepoint for '{text[i]}'");
                continue;
            }

            int index = Raylib.GetGlyphIndex(font, codepoint);

            if (font.Glyphs[index].AdvanceX == 0)
            {
                width += font.Recs[index].Width;
            }
            else
            {
                width += font.Glyphs[index].AdvanceX;
            }
        }

        return (int)width;
    }

    static Dictionary<string, string> RemoveControlCharsCache = [];
    public static string RemoveControlCharacters(string input)
    {
        if (RemoveControlCharsCache.ContainsKey(input))
        {
            return RemoveControlCharsCache[input];
        }

        const int FirstPrintChar = 32;
        const int Delete = 127;

        bool controlChars = false;
        foreach (var c in input)
        {
            if (c < FirstPrintChar || c == Delete)
            {
                controlChars = true;
            }
        }

        if (!controlChars)
        {
            RemoveControlCharsCache[input] = input;
            return input;
        }

        StringBuilder sb = new();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c < FirstPrintChar || c == Delete)
            {
                sb.Append('?');
                continue;
            }

            sb.Append(c);
        }

        string result = sb.ToString();
        RemoveControlCharsCache[input] = result;
        return result;
    }

    public static int Mod(int x, int m)
    {
        return ((x % m) + m) % m;
    }   
}

public static class StringHelper
{
    /// <summary>
    /// Get the index of the <paramref name="n"/>th occurrence of <paramref name="search"/> in the string.
    /// Returns -1 if there is not that many occurrences in the string
    /// </summary>
    public static int IndexOfInstance(this string str, char search, int n)
    {
        var indices = str.GetIndicesOf(search);
        if (n-1 < indices.Count)
        {
            return indices[n-1];
        }
        return -1;
    }

    public static List<int> GetIndicesOf(this string str, char search)
    {
        var c = str.ToCharArray();
        List<int> indices = new();

        for (int i = 0; i < c.Length; i++)
        {
            if (c[i] == search)
            {
                indices.Add(i);
            }
        }

        return indices;
    }
}