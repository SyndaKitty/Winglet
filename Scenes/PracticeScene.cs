using Raylib_cs;
using System.Numerics;
using System.Text;
using static Raylib_cs.Raylib;

public class PracticeScene : Scene
{
    const string Tag = "PracticeScene";

    Font primaryFont = Shared.GetFont(Shared.PrimaryFontFile, 80);
    string lessonPath;
    List<Word> words;

    int wordIndex = 0;

    public PracticeScene(string lessonFilepath)
    {
        lessonPath = lessonFilepath;
    }

    public void Load() 
    {
        var lesson = Lesson.Load(lessonPath);
        if (lesson is null)
        {
            Log.Error(Tag, "Unable to load lesson scene");
            throw new ApplicationException($"Unable to load lesson {lessonPath}", null);
        }
        words = lesson.Words ?? [];
    }

    public void Unload() { }
    
    public void Draw() 
    {
        Vector2 origin = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2 - primaryFont.BaseSize / 2);
        Vector2 cursor = origin;

        Word word = words[wordIndex];
        DrawRectangle((int)origin.X + GetWordWidth(word, true), (int)origin.Y, 2, primaryFont.BaseSize, Shared.AccentColor);
        
        for (int i = wordIndex; i < words.Count; i++)
        {
            word = words[i];
            int width = DrawWord(word, cursor);
            cursor.X += width;

            if (cursor.X > GetScreenWidth()) break;
        }

        cursor = origin;
        for (int i = wordIndex - 1; i >= 0 && i < words.Count; i--)
        {
            word = words[i];
            cursor.X -= GetWordWidth(word);
            DrawWord(word, cursor);

            if (cursor.X < 0) break;
        }

        ClearBackground(Shared.BackgroundColor);
    }

    public void Update() 
    {
        float dy = GetMouseWheelMoveV().Y;
        if (dy != 0)
        {
            int newFontSize = primaryFont.BaseSize + (int)dy;
            newFontSize = Math.Clamp(newFontSize, 10, 200);

            primaryFont = Shared.GetFont(Shared.PrimaryFontFile, newFontSize);
        }

        int key = GetCharPressed();
        while (key > 0)
        {
            Console.WriteLine($"{key} {(char)key}");
            if (key == ' ')
            {
                if (words[wordIndex].InputBuffer.Length > 0)
                {
                    wordIndex = Math.Min(wordIndex + 1, words.Count - 1);
                }
            }
            else
            {
                words[wordIndex].InputBuffer.Append((char)key);
            }
            key = GetCharPressed();
        }

        key = GetKeyPressed();
        while (key > 0)
        {
            if (key == (int)KeyboardKey.Backspace)
            {
                Backspace();
            }
            key = GetKeyPressed();
        }

        //int key = GetKeyPressed();
        //while (key > 0)
        //{
        //    if (key == (int)KeyboardKey.Backspace)
        //    {
        //        Console.WriteLine("Backspace");
        //        Backspace();
        //    }
        //    else if ((char)key == ' ')
        //    {
        //        Console.WriteLine("' '");
        //        if (words[wordIndex].InputBuffer.Length > 0)
        //        {
        //            wordIndex = Math.Min(wordIndex + 1, words.Count - 1);
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine(key + " " + (char)key + " " + ((char)key).ToString().ToLower());
        //        words[wordIndex].InputBuffer.Append(((char)key).ToString().ToLower());
        //    }
        //    key = GetKeyPressed();
        //}

        if (IsKeyPressedRepeat(KeyboardKey.Backspace))
        {
            Backspace();
        }
    }

    void Backspace()
    {
        var word = words[wordIndex];
        if (word.InputBuffer.Length == 0)
        {
            // Go to previous word
            wordIndex = Math.Max(0, wordIndex - 1);
        }
        else
        {
            // Remove last character
            StringBuilder newString = new();
            newString.Append(word.InputBuffer.ToString().Substring(0, word.InputBuffer.Length - 1));

            word.InputBuffer = newString;
        }
    }

    /// <summary>
    /// Get pixel width of word, including space
    /// </summary>
    /// <param name="word">The word to measure</param>
    /// <param name="truncate">only consider the number of characters in the input buffer</param>
    /// <returns>pixel width of word</returns>
    int GetWordWidth(Word word, bool truncate = false)
    {
        string text = word.Text;
        var inputBuffer = word.InputBuffer;
        if (inputBuffer.Length > word.Text.Length)
        {
            var extraInput = inputBuffer.ToString().Substring(word.Text.Length);
            text += extraInput;
        }

        if (truncate)
        {
            return GetTextWidth(text.Substring(0, word.InputBuffer.Length));
        }
        return GetTextWidth(text + " ");
    }

    int DrawWord(Word word, Vector2 pos)
    {
        string text = word.Text;
        StringBuilder buffer = word.InputBuffer;

        int totalWidth = 0;

        for (int i = 0; i < Math.Max(text.Length, buffer.Length); i++)
        {
            bool textContains = text.Length > i;
            bool bufferContains = buffer.Length > i;
            float fontSize = primaryFont.BaseSize;

            string str = "";
            Color color = Color.Black;

            if (textContains && !bufferContains)
            {
                str = text[i].ToString();
                color = Shared.TextColor;
            }
            else if (bufferContains && !textContains)
            {
                str = buffer[i].ToString();
                color = Shared.AltErrTextColor;
            }
            else if (buffer[i] != text[i])
            {
                str = text[i].ToString();
                color = Shared.ErrTextColor;
            }
            else // Match
            {
                str = buffer[i].ToString();
                color = Shared.AltTextColor;
            }
            DrawTextEx(primaryFont, str, pos, fontSize, 0, color);

            int width = GetTextWidth(str);
            pos.X += width;
            totalWidth += width;
        }

        return totalWidth + GetTextWidth(" ");
    }
    
    unsafe int GetTextWidth(string text)
    {
        float width = 0;

        for (int i = 0; i < text.Length; i++)
        {
            int codepointByteCount = 0;
            int codepoint = GetCodepoint(text[i].ToString(), ref codepointByteCount);
            int index = GetGlyphIndex(primaryFont, codepoint);

            if (primaryFont.Glyphs[index].AdvanceX == 0)
            {
                width += primaryFont.Recs[index].Width;
            }
            else
            {
                width += primaryFont.Glyphs[index].AdvanceX;
            }
        }

        return (int)width;
    }
}