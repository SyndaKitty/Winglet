using Raylib_cs;

public class UI
{
    public int FontSize = 40;

    List<Toast> toasts = [];
    Font primaryFont;

    public UI()
    {
        primaryFont = Shared.GetFont(Shared.PrimaryFontFile, FontSize);
    }

    public void AddToast(string message, float duration)
    {
        toasts.Add(new Toast(message, duration));
    }

    public void Draw() 
    {
        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();

        foreach (var toast in toasts)
        {
        }
    }
}

public class Toast
{
    public string Message;
    public float Duration;

    public Toast(string message, float duration)
    {
        Message = message;
        Duration = duration;
    }
}