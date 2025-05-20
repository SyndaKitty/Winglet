using System.Text;

public class Word
{
    public string Text;
    public string? InputExplanation;
    public StringBuilder InputBuffer;
    public bool Counted;

    public Word(string text)
    {
        Text = text;
        InputBuffer = new();
    }

    public override string ToString()
    {
        return Text;
    }
}