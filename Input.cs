using Raylib_cs;

public class Input
{
    Mode inputMode;
    PloverServer server;

    public delegate void TextTypedHandler(string text);
    public event TextTypedHandler? OnTextTyped;

    public delegate void BackspaceHandler(int count);
    public event BackspaceHandler? OnBackspace;

    public Input(PloverServer server)
    {
        this.server = server;
    }

    public void SetInputMode(Mode inputMode)
    {
        this.inputMode = inputMode;
        if (inputMode == Mode.Plover)
        {
            server.OnSendString += HandleString;
            server.OnSendBackspace += HandleBackspace;
        }
        else
        {
            server.OnSendString -= HandleString;
            server.OnSendBackspace -= HandleBackspace;
        }
    }

    public void Update()
    {
        if (inputMode == Mode.Keyboard)
        {
            KeyboardInput();
        }
    }

    void KeyboardInput()
    {
        int key = Raylib.GetCharPressed();
        while (key > 0)
        {
            OnTextTyped?.Invoke(((char)key).ToString());
            key = Raylib.GetCharPressed();
        }

        key = Raylib.GetKeyPressed();
        while (key > 0)
        {
            if (key == (int)KeyboardKey.Backspace)
            {
                OnBackspace?.Invoke(1);
            }
            key = Raylib.GetKeyPressed();
        }

        if (Raylib.IsKeyPressedRepeat(KeyboardKey.Backspace))
        {
            OnBackspace?.Invoke(1);
        }
    }

    void HandleString(PloverString str)
    {
        OnTextTyped?.Invoke(str.Text);
    }

    void HandleBackspace(PloverBackspace backspace)
    {
        OnBackspace?.Invoke(backspace.Count);
    }

    public enum Mode
    {
        Plover,
        Keyboard,
    }
}