using System.Text;

public class Word
{
    public string Text;
    public string? InputExplanation;
    public StringBuilder InputBuffer;

    public Word(string text, string? input)
    {
        Text = text;
        InputExplanation = input;
        InputBuffer = new();
    }
}