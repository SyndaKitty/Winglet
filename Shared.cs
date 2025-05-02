using Raylib_cs;
using System.IO;
using System.Runtime.InteropServices;
using Tomlyn;

public static class Shared
{
    public static Color BackgroundColor = new Color(51, 58, 69);
    public static Color PanelColor = new Color(44, 49, 59);
    public static Color AccentColor = new Color(244, 76, 127);
    
    public static Color TextColor = new Color(147, 158, 174);
    public static Color AltTextColor = new Color(233, 236, 240);
    
    public static Color ErrTextColor = new Color(218, 51, 51);
    public static Color AltErrTextColor = new Color(121, 23, 23);

    public static string PrimaryFontFile = "Resources/Hack-Regular.ttf";
    public static string SecondaryFontFile = "Resources/Roboto-Medium.ttf";

    public static Dictionary<(string, int), Font> FontCache = [];
    
    public static UserSettings UserSettings = new();

    const string Tag = "Shared";

    public static Font GetFont(string fontFile, int fontSize)
    {
        var key = (fontFile, fontSize);
        if (FontCache.ContainsKey(key))
        {
            return FontCache[key];
        }
        var font = Raylib.LoadFontEx(fontFile, fontSize, null, 0);
        FontCache[key] = font;
        return font;
    }

    public static string GetAppdataPath()
    {
        const string AppFolder = "Wingman";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string? path = Environment.GetEnvironmentVariable("AppData");

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

    public static void LoadUserSettings()
    {
        var path = Path.Combine(GetAppdataPath(), "Settings.toml");
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
        int version = 0;
        while (true)
        {
            if (version > 0)
            {
                newPath = path + $"_{version}.old";
            }
            else
            {
                newPath = path + ".old";
            }

            if (!File.Exists(newPath))
            {
                break;
            }
            version++;
        }
        
        Log.Info(Tag, $"Archiving old settings file to {newPath}");
        File.Move(path, newPath, true);
    }
}