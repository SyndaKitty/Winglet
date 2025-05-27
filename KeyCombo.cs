public class KeyCombo
{
    const string Tag = "KeyCombo";

    Queue<string> inputQueue;
    string[] watchPhrase;

    public delegate void HandleMatch();
    public event HandleMatch? OnMatch;

    public KeyCombo(params string[] watchPhrase)
    {
        this.watchPhrase = watchPhrase;
        inputQueue = [];

        for (int i = 0; i < watchPhrase.Length; i++)
        {
            inputQueue.Enqueue("");
        }

        if (watchPhrase.Length == 0)
        {
            Log.Error(Tag, "Invalid empty watch phrase registered");
            return;
        }
        Log.Info(Tag, $"Registered watch phrase: {string.Join(',', watchPhrase)}");

        Input.OnStroke += OnStroke;
    }

    ~KeyCombo()
    {
        Input.OnStroke -= OnStroke;
    }

    void OnStroke(string stroke)
    {
        inputQueue.Dequeue();
        inputQueue.Enqueue(stroke);

        var input = inputQueue.GetEnumerator();
        for (int i = 0; i < watchPhrase.Length; i++)
        {
            input.MoveNext();
            var inputStroke = input.Current;
            if (watchPhrase[i] != inputStroke) return;
        }

        OnMatch?.Invoke();
    }
}