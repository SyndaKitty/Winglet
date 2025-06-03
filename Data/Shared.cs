using Raylib_cs;
using System.Runtime.InteropServices;
using Tomlyn;

public static class Shared
{
    const string Tag = "Shared";
    
    public static Color BackgroundColor = new Color(51, 58, 69);
    public static Color PanelColor = new Color(44, 49, 59);
    public static Color AccentColor = new Color(244, 76, 127);
    
    public static Color TextColor = new Color(147, 158, 174);
    public static Color AltTextColor = new Color(233, 236, 240);
    
    public static Color ErrTextColor = new Color(218, 51, 51);
    public static Color AltErrTextColor = new Color(121, 23, 23);

    public static Color WarningTextColor = new Color(238, 242, 125);

    public static string PrimaryFontFile = "Hack-Regular.ttf";
    public static string SecondaryFontFile = "Hack-Regular.ttf";

    public static UserSettings UserSettings = new();

    public const int SlideSpeed = 15;

    static Dictionary<(string, int), Font> fontCache = [];
    public static Font GetFont(string filename, int fontSize)
    {
        filename = $"Resources/Fonts/{filename}";
        var key = (filename, fontSize);
        if (fontCache.ContainsKey(key))
        {
            return fontCache[key];
        }

        Log.Info(Tag, $"Loading font {filename}");
        var font = Raylib.LoadFontEx(filename, fontSize, null, 0);
        fontCache[key] = font;
        return font;
    }

    static Dictionary<string, Image> imageCache = [];
    public static Image LoadImage(string filename)
    {
        filename = $"Resources/Images/{filename}";
        
        if (imageCache.ContainsKey(filename))
        {
            return imageCache[filename];
        }

        Log.Info(Tag, $"Loading image {filename}");
        var image = Raylib.LoadImage(filename);
        imageCache.Add(filename, image);

        return image;
    }

    static Dictionary<string, Texture2D> textureCache = [];
    public static Texture2D LoadTexture(string filename)
    {
        filename = $"Resources/Images/{filename}";

        if (textureCache.ContainsKey(filename))
        {
            return textureCache[filename];
        }

        Log.Info(Tag, $"Loading texture {filename}");
        var tex = Raylib.LoadTexture(filename);
        textureCache.Add(filename, tex);

        return tex;
    }

    static Dictionary<string, Sound> soundCache = [];
    public static Sound LoadSound(string filename)
    {
        filename = $"Resources/Sounds/{filename}";

        if (soundCache.ContainsKey(filename))
        {
            return soundCache[filename];
        }
        Log.Info(Tag, $"Loading sound {filename}");
        
        var sound = Raylib.LoadSound(filename);
        soundCache.Add(filename, sound);

        return sound;
    }


    public static SpriteSheet LoadSpriteSheet(string filename, int frameCount)
    {
        return new SpriteSheet 
        {
            Texture = LoadTexture(filename),
            FrameCount = frameCount
        };
    }

    public static string GetApplicationPath()
    {
        const string AppFolder = "Wingman";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string? path = Environment.GetEnvironmentVariable("APPDATA");

            // Fallback if %AppData% is not set
            if (path is null)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }

            return Path.Combine(path, AppFolder);
        }
        else
        {
            string? path = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (path is null)
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
            }
            
            return Path.Combine(path, AppFolder);
        }
    }

    public static string? GetPloverPath()
    {
        var local = GetLocalAppdataPath();
        if (local is null) return null;
        return Path.Combine(local, "plover/plover");
    }

    public static string? GetLocalAppdataPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string? path = Environment.GetEnvironmentVariable("LOCALAPPDATA");

            // Fallback if %AppData% is not set
            if (path is null)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }

            return path;
        }
        else
        {
            // TODO: Can be anywhere :loldunno:
            Log.Error(Tag, "Getting local appdata path is not implemented on *nix systems");
            return null;
        }
    }

    public static void LoadUserSettings()
    {
        var path = Path.Combine(GetApplicationPath(), "Settings.toml");
        if (File.Exists(path))
        {
            Log.Info(Tag, $"Loading user settings from {path}");
            try
            {
                UserSettings = Toml.ToModel<UserSettings>(File.ReadAllText(path));
                return;
            }
            catch (Exception e)
            {
                Log.Error(Tag, $"Failed to load user settings: {e.Message}");

                ArchiveSettingsFile(path);
            }
        }
        
        Log.Info(Tag, $"Creating new user settings file at {path}");
        UserSettings = new UserSettings();
        File.WriteAllText(path, Toml.FromModel(UserSettings));
    }

    static void ArchiveSettingsFile(string path)
    {
        string newPath;
        for (int version = 0; ; version++)
        {
            if (version > 0)
            {
                newPath = path + $"_{version}.old";
            }
            else
            {
                newPath = path + ".old";
            }

            if (!File.Exists(newPath)) break;
        }
        
        Log.Info(Tag, $"Archiving old settings file to {newPath}");
        File.Move(path, newPath, true);
    }

    public static string ResultFilePath => Path.Combine(GetApplicationPath(), "ResultLog.txt");

    static string[] gayColorsHex = ["E50000", "FF8D00", "FFEE00", "028121", "004CFF", "760088"];
    static Color[]? _gayColors;
    public static Color[] GayColors { 
        get 
        { 
            if (_gayColors == null)
            {
                _gayColors = gayColorsHex.Select(Util.HexColor).ToArray();
            }

            return _gayColors;
        } 
    }

    static string[] rainbowColorsHex = ["E50000", "FF8D00", "FFEE00", "028121", "004CFF", "5541AD", "8E018E"];
    static Color[]? _rainbowColors;
    public static Color[] RainbowColors
    {
        get
        {
            if (_rainbowColors == null)
            {
                _rainbowColors = rainbowColorsHex.Select(Util.HexColor).ToArray();
            }

            return _rainbowColors;
        }
    }

    static string[] pastelRainbowColorsHex = ["FAB7DA", "F0C0AA", "F0E78B", "A1EBCE", "A4EAF0", "BBC1F2", "B8A5E0"];
    static Color[]? _pastelRainbowColors;
    public static Color[] PastelRainbowColors
    {
        get
        {
            if (_pastelRainbowColors == null)
            {
                _pastelRainbowColors = pastelRainbowColorsHex.Select(Util.HexColor).ToArray();
            }

            return _pastelRainbowColors;
        }
    }

    static string[] transColorsHex = ["73D7EE", "FFAFC7", "FFFFFF", "FFAFC7", "73D7EE"];
    static Color[]? _transColors;
    public static Color[] TransColors
    {
        get
        {
            if (_transColors == null)
            {
                _transColors = transColorsHex.Select(Util.HexColor).ToArray();
            }

            return _transColors;
        }
    }
}