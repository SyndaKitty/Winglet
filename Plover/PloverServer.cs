using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Wraps a raw Plover connections and performs translation of the json messages.
/// Callbacks are provided to handle the messages.
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

    PloverConnection connection;
    public PloverServer()
    {
        connection = new();
    }

    public async Task<bool> Connect()
    {
        connection.OnMessage += ReceiveMessage;
        bool connected = await connection.Connect();
        if (!connected)
        {
            return false;
        }

        connection.BeginReading();
        Log.Info(Tag, "Began reading successfully");
        return true;
    }

    public void ReceiveMessage(string message)
    {
        var deserialized = JsonConvert.DeserializeObject(message);
        if (deserialized is null || deserialized is not JObject)
        {
            Log.Error(Tag, "Failed to deserialize message");
            return;
        }
        
        var json = (JObject)deserialized;
        Translate(json);
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