using Raylib_cs;

public static class Input
{
    const string Tag = "Input";
    
    static PloverServer? server;

    public delegate void TextTypedHandler(string text);
    public static event TextTypedHandler? OnTextTyped;

    public delegate void BackspaceHandler(int count);
    public static event BackspaceHandler? OnBackspace;

    public delegate void StenoKeysHandler(List<string> keys);
    public static event StenoKeysHandler? OnStenoKeys;

    public delegate void StrokeHandler(string stroke);
    public static event StrokeHandler? OnStroke;

    public delegate void PaperHandler(string paper);
    public static event PaperHandler? OnPaper;

    // Ensure to call before server connection attempt
    public static void SetServer(PloverServer server)
    {
        if (Input.server != null)
        {
            server.OnSendString -= HandleString;
            server.OnSendBackspace -= HandleBackspace;
            server.OnSendStroke -= HandleStroke;
        }

        Input.server = server; 
        server.OnSendString += HandleString;
        server.OnSendBackspace += HandleBackspace;
        server.OnSendStroke += HandleStroke;
    }

    static void HandleString(PloverString str)
    {
        OnTextTyped?.Invoke(str.Text);
    }

    static void HandleBackspace(PloverBackspace backspace)
    {
        OnBackspace?.Invoke(backspace.Count);
    }

    static void HandleStroke(PloverStroke stroke)
    {
        OnStenoKeys?.Invoke(stroke.Keys);
        OnStroke?.Invoke(stroke.Rtfcre);
        OnPaper?.Invoke(stroke.Paper);
    }
}