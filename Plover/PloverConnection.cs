using NaCl;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Responsible for raw connection to the Plover server.
/// Does not deal with Plover logic, but rather getting the raw data.
/// Assign callback to receive data
/// </summary>
public class PloverConnection
{
    const string Tag = "PloverConnection";

    PloverServerConfig serverConfig;
    RandomNumberGenerator rnd;
    byte[] privateKey;
    byte[] publicKey;

    Curve25519XSalsa20Poly1305? mailbox;
    ClientWebSocket? webSocket;

    public delegate void MessageReceivedHandler(string message);
    public event MessageReceivedHandler? OnMessage;

    public delegate void OnConnectHandler();
    public event OnConnectHandler? OnConnect;

    public delegate void OnDisconnectHandler();
    public event OnDisconnectHandler? OnDisconnect;

    public string ServerHost
    {
        get { return serverConfig.host; }
        set { serverConfig.host = value; }
    }

    public string ServerPort {
        get { return serverConfig.port; }
        set { serverConfig.port = value; }
    }

    public string ServerPublicKey {
        get { return serverConfig.public_key; }
        set { serverConfig.public_key = value; }
    }

    public PloverConnection()
    {
        serverConfig = GetPloverServerConfig();

        Curve25519XSalsa20Poly1305.KeyPair(out privateKey, out publicKey);

        // Generate random nonce
        rnd = RandomNumberGenerator.Create();
    }

    public async Task<bool> Connect()
    {
        Log.Info(Tag, "Attempting to connect to plover server");

        if (string.IsNullOrEmpty(ServerPort) || string.IsNullOrEmpty(ServerPublicKey) || string.IsNullOrEmpty(ServerHost))
        {
            Log.Error(Tag, "Plover server config is not set");
            return false;
        }

        mailbox = new Curve25519XSalsa20Poly1305(privateKey, Convert.FromHexString(ServerPublicKey));
        webSocket = new();
        webSocket.Options.SetRequestHeader("Origin", ServerHost);

        var publicKeyHex = Convert.ToHexString(publicKey);
        var encryptedMessage = Uri.EscapeDataString(EncryptAndPackMessage("{}"));
        Uri url = new Uri($"ws://{ServerHost}:{ServerPort}/websocket?publicKey={publicKeyHex}&encryptedMessage={encryptedMessage}");
        Log.Info(Tag, $"Connecting to {url}");

        try
        {
            await webSocket.ConnectAsync(url, CancellationToken.None);
            Log.Info(Tag, "Connected to Plover");
            OnConnect?.Invoke();

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(Tag, $"Error when connecting to plover server: {ex.ToString()}");
            return false;
        }
    }

    public void BeginReading()
    {
        Task.Run(ReadLoop);
    }

    async Task ReadLoop()
    {
        byte[] buffer = new byte[2048];

        if (webSocket is null)
        {
            Log.Error(Tag, "No active connection to Plover server.");
            return;
        }

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                Log.Info(Tag, "Server requested close. Closing...");
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                break;
            }

            // Bytes need reinterpreted as base64 string via UTF8 encoding, then converted back to bytes
            var rawBytes = buffer.Take(result.Count).ToArray();
            var base64String = Encoding.UTF8.GetString(rawBytes);
            var actualBytes = Convert.FromBase64String(base64String);

            var serverNonce = actualBytes.Take(Curve25519XSalsa20Poly1305.NonceLength).ToArray();
            var encryptedMessage = actualBytes.Skip(Curve25519XSalsa20Poly1305.NonceLength).ToArray();

            byte[] decipheredMessage = new byte[encryptedMessage.Length - Curve25519XSalsa20Poly1305.TagLength];
            if (mailbox!.TryDecrypt(decipheredMessage, encryptedMessage, serverNonce))
            {
                string message = Encoding.UTF8.GetString(decipheredMessage);
                
                Log.Info(Tag, $"Plover: {message}");
                OnMessage?.Invoke(message);
            }
            else
            {
                Log.Error(Tag, "Failed to decrypt message");
            }
        }

        Log.Info(Tag, "Disconnected from Plover");
        OnDisconnect?.Invoke();
    }

    async Task SendMessage(ClientWebSocket webSocket, string message)
    {
        var encryptedMessage = EncryptAndPackMessage(message);
        var bytes = Encoding.UTF8.GetBytes(encryptedMessage);
        await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, new());
    }

    string EncryptAndPackMessage(string message)
    {
        byte[] cipher = new byte[message.Length + Curve25519XSalsa20Poly1305.TagLength];
        
        var nonce = new byte[Curve25519XSalsa20Poly1305.NonceLength];
        rnd.GetBytes(nonce);

        mailbox!.Encrypt(cipher, Encoding.UTF8.GetBytes(message), nonce);

        List<byte> fullMessage = [];
        fullMessage.AddRange(nonce);
        fullMessage.AddRange(cipher);

        Log.Trace(Tag, $"Outgoing Nonce: {Convert.ToBase64String(nonce)}");
        Log.Trace(Tag, $"Outgoing Cipher: {Convert.ToBase64String(cipher)}");

        return Convert.ToBase64String(fullMessage.ToArray());
    }

    PloverServerConfig GetPloverServerConfig()
    {
        var ploverPath = Shared.GetPloverPath();
        if (ploverPath is null) return new();
        
        const string ConfigFileName = "plover_websocket_server_config.json";
        var serverConfigPath = Path.Combine(ploverPath, ConfigFileName);

        try
        {
            var serverConfigText = File.ReadAllText(serverConfigPath);

            var serverJson = JsonConvert.DeserializeObject<PloverServerConfig>(serverConfigText);
            if (serverJson is null)
            {
                throw new InvalidDataException("Unable to parse json");
            }

            return serverJson;
        }
        catch (Exception ex)
        {
            Log.Error(Tag, $"Unable to read plover server config file {serverConfigPath}: {ex}");
        }

        return new();
    }
}