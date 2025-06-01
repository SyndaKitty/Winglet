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

    public static void Shuffle<T>(IList<T> list)
    {
        var rng = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public static float InvLerp(float a, float b, float val)
    {
        return (val - a) / (b - a);
    }

    public static int ConsistentStringHash(string? str)
    {
        if (str == null || str.Length == 0) return 0;
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    static Random? rand;
    public static bool RandomChance(float chance)
    {
        if (rand == null) rand = new Random();
        return rand.NextDouble() < chance;
    }

    public static float Map(float value, float fromStart, float fromEnd, float toStart, float toEnd)
    {
        if (fromStart == fromEnd) return Lerp(toStart, toEnd, 0.5f);

        float t = InvLerp(fromStart, fromEnd, value);
        t = Clamp01(t);
        return Lerp(toStart, toEnd, t);
    }

    public static Color LerpColor(Color a, Color b, float t)
    {
        t = Clamp01(t);

        var av = ToVector(a);
        var bv = ToVector(b);
        var lerped = Vector3.Lerp(av, bv, t);

        return new Color(lerped.X, lerped.Y, lerped.Z);
    }

    public static Vector3 ToVector(Color c)
    {
        return new Vector3(c.R / 255f, c.G / 255f, c.B / 255f);
    }

    public static float Clamp01(float t)
    {
        return Math.Max(0, Math.Min(1, t));
    }

    public static float Clamp(float a, float b, float v)
    {
        var from = Math.Min(a, b);
        var to = Math.Max(a, b);
        return Math.Max(from, Math.Min(to, v));
    }

    public static Color HexColor(string hex)
    {
        string originalHex = hex;
        hex = hex.Replace("#", "");
        if (hex.Length != 6 && hex.Length != 8)
        {
            Log.Error(Tag, $"Invalid hexcode length: '{originalHex}' with length of {hex.Length}. Must be 6 or 8 characters");
        }

        List<byte> colorVals = [];
        try
        {
            for (int i = 0; i < hex.Length; i += 2)
            {
                colorVals.Add(Convert.FromHexString(hex.Substring(i, 2)).First());
            }    
        }
        catch
        {
            Log.Error(Tag, $"Invalid hexcode: {originalHex}");
        }

        if (colorVals.Count == 3)
        {
            return new Color(colorVals[0], colorVals[1], colorVals[2]);
        }
        return new Color(colorVals[0], colorVals[1], colorVals[2], colorVals[3]);
    }


    // Note: Something is wrong about this implementation
    /// <summary>
    /// Convert RGB Color to normalized HSV Vector
    /// </summary>
    /// <param name="c">Color with RGB values</param>
    /// <returns>Normalized HSV vector</returns>
    //public static Vector3 ColorToHSV(Color c)
    //{
    //    // https://math.stackexchange.com/questions/556341/rgb-to-hsv-color-conversion-algorithm
    //    float r = c.R / 255f;
    //    float g = c.G / 255f;
    //    float b = c.B / 255f;
    //    float maxC = Math.Max(r, Math.Max(g, b));
    //    float minC = Math.Min(r, Math.Min(g, b));

    //    if (minC == maxC)
    //    {
    //        return new(0, 0, maxC);
    //    }
    //    float delta = maxC - minC;
    //    float rc = (maxC - r) / delta;
    //    float gc = (maxC - g) / delta;
    //    float bc = (maxC - b) / delta;

    //    float H; // [0,1]
    //    if (r == maxC) H = 0f + bc - gc;
    //    if (g == maxC) H = 2f + rc - bc;
    //    else H = 4f + gc - rc;

    //    H = (H / 6f) % 1f;

    //    float S = (maxC - minC) / maxC;
    //    float V = maxC;

    //    return new(H, S, V);
    //}

    // https://stackoverflow.com/questions/3018313/algorithm-to-convert-rgb-to-hsv-and-hsv-to-rgb-in-range-0-255-for-both
    //public static Color HSVToColor(Vector3 hsv)
    //{
    //    (float h, float s, float v) = (hsv.X, hsv.Y, hsv.Z);
    //    if (s < 0) new Color(v, v, v);

    //    h *= 360f;
    //    float hh = h;
    //    if (hh >= 360f) hh = 0;
    //    hh /= 60f;
    //    long i = (long)hh;
    //    float ff = hh - i;
    //    float p = v * (1f - s);
    //    float q = v * (1f - s * ff);
    //    float t = v * (1f - s * (1f - ff));

    //    switch (i)
    //    {
    //        case 0:
    //            return new Color(v, t, p);
    //        case 1:
    //            return new Color(q, v, p);
    //        case 2:
    //            return new Color(p, v, t);
    //        case 3:     
    //            return new Color(p, q, v);
    //        case 4:
    //            return new Color(t, p, v);
    //        default:
    //            return new Color(v, p, q);
    //    }
    //}
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