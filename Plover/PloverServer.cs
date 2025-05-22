using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

/// <summary>
/// Wraps a raw Plover connections and performs translation of the json messages.
/// Callbacks are provided to handle the messages.
/// DispatchMessages must be called to invoke the queued callbacks.
/// </summary>
public class PloverServer
{
    public const string Tag = "PloverServer";

    public delegate void SendStringHandler(PloverString str);
    public event SendStringHandler? OnSendString;

    public delegate void SendStrokeHandler(PloverStroke stroke);
    public event SendStrokeHandler? OnSendStroke;

    public delegate void SendBackspaceHandler(PloverBackspace backspace);
    public event SendBackspaceHandler? OnSendBackspace;

    public delegate void OnConnectHandler();
    public event OnConnectHandler? OnConnect;

    public delegate void OnDisconnectHandler();
    public event OnDisconnectHandler? OnDisconnect;

    ConcurrentQueue<Action> messageQueue;
    PloverConnection connection;
    Task? readTask;
    SemaphoreSlim connectingSemaphore;

    public PloverServer()
    {
        messageQueue = new();
        connection = new();
        connectingSemaphore = new(1, 1);

        // Enqueue the event invocation to a queue, so the main thread can process it
        connection.OnConnect += () => messageQueue.Enqueue(() => OnConnect?.Invoke());
        connection.OnDisconnect += () => messageQueue.Enqueue(() => OnDisconnect?.Invoke());
        connection.OnMessage += (string message) => messageQueue.Enqueue(() => ReceiveMessage(message));
    }

    public async Task<bool> Connect()
    {
        await connectingSemaphore.WaitAsync();
        try
        {
            if (readTask != null)
            {
                Log.Info(Tag, "Closing existing connection");
                Close();
            }

            bool connected = await connection.Connect();
            if (!connected)
            {
                return false;
            }
            readTask = connection.BeginReading();
        }
        catch (Exception ex)
        {
            Log.Error(Tag, $"Error encountered when connecting to Plover: {ex}");
            return false;
        }
        finally
        {
            connectingSemaphore.Release();
        }
        
        Log.Info(Tag, "Began reading successfully");
        return true;
    }

    public void Close()
    {
        connection.Close();
        readTask?.Wait();
        Log.Info(Tag, "Connection closed");
    }

    public void DispatchMessages()
    {
        while (messageQueue.TryDequeue(out var message))
        {
            message.Invoke();
        }
    }

    void ReceiveMessage(string message)
    {
        var deserialized = JsonConvert.DeserializeObject(message);
        if (deserialized is JObject j)
        {
            Translate(j);
        }
        else
        {
            Log.Error(Tag, "Failed to deserialize message");
        }
    }

    void Translate(JObject json)
    {
        if (json.ContainsKey("send_string"))
        {
            SendString(json);
        }
        else if (json.ContainsKey("send_backspaces"))
        {
            SendBackspaces(json);
        }
        else if (json.ContainsKey("stroked"))
        {
            SendStroke(json);
        }
    }

    void SendString(JObject json)
    {
        OnSendString?.Invoke(
            new PloverString
            {
                Text = json["send_string"]!.ToString()
            }
        );
    }

    void SendStroke(JObject json)
    {
        List<string> keys = json["keys"]?.ToObject<List<string>>() ?? [];

        OnSendStroke?.Invoke(
            new PloverStroke
            {
                Keys = keys,
                StrokedJson = json["stroked"]?.ToString() ?? "",
                Rtfcre = json["rtfcre"]?.ToString() ?? "",
                Paper = json["paper"]?.ToString() ?? ""
            }
        );
    }

    void SendBackspaces(JObject json)
    {
        OnSendBackspace?.Invoke(
            new PloverBackspace
            {
                Count = json["send_backspaces"]?.ToObject<int>() ?? 0
            }
        );
    }
}

public struct PloverString
{
    public string Text;
}

public struct PloverStroke
{
    public List<string> Keys;
    public string StrokedJson;
    public string Rtfcre;
    public string Paper;
}

public struct PloverBackspace
{
    public int Count;
}