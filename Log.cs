using System.Collections.Concurrent;

public enum Severity
{
    All,
    Trace,
    Info,
    Warning,
    Error,
    Fatal,
    None
}

public static class Log
{
    static string logFilePath;
    static ConcurrentQueue<LogMessageInfo> logQueue;
    static Thread logThread;
    static bool isLogging;
    static List<string> whitelistTags = new();
    static List<string> blacklistTags = new();
    static Severity Severity = Severity.Info;

    static List<string> SeverityTag = [
        "[All  ]",
        "[Trace]",
        "[Info ]",
        "[Warn ]",
        "[Error]",
        "[Fatal]",
        "[None ]"
    ];

    static Log()
    {
        string currentTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"log_{currentTime}.txt";

        string appData = Shared.GetApplicationPath();
        Directory.CreateDirectory(appData);

        string logFileDir = Path.Combine(appData, "Logs");
        Directory.CreateDirectory(logFileDir);
        
        logFilePath = Path.Combine(logFileDir, fileName);

        logQueue = new();
        isLogging = true;

        using (StreamWriter writer = File.AppendText(logFilePath))
        {
            writer.WriteLine($"Log file created at: {DateTime.Now}");
            writer.AutoFlush = true;
        }
        Console.WriteLine($"Log file created at: {logFilePath}");

        logThread = new Thread(LogThread);
        logThread.Start();
    }

    public static void Info(string tag, string message, bool ignoreFilter = false)
    {
        EnqueueLog(Severity.Info, tag, message, ignoreFilter);
    }

    public static void Trace(string tag, string message, bool ignoreFilter = false)
    {
        EnqueueLog(Severity.Trace, tag, message, ignoreFilter);
    }

    public static void Warning(string tag, string message, bool ignoreFilter = false)
    {
        EnqueueLog(Severity.Warning, tag, message, ignoreFilter);
    }

    public static void Error(string tag, string message, bool ignoreFilter = false)
    {
        EnqueueLog(Severity.Error, tag, message, ignoreFilter);
    }

    public static void Fatal(string tag, string message, bool ignoreFilter = false)
    {
        EnqueueLog(Severity.Fatal, tag, message, ignoreFilter);
    }

    public static void SetSeverity(Severity severity)
    {
        Severity = severity;
    }

    public static void SetWhitelist(List<string> tags)
    {
        whitelistTags = tags;
    }

    public static void SetBlacklist(List<string> tags)
    {
        blacklistTags = tags;
    }

    static void EnqueueLog(Severity severity, string tag, string message, bool ignoreFilter)
    {
        string currentTime = DateTime.Now.ToString("HH:mm:ss.fff");
        string logMessage = $"[{currentTime}] {SeverityTag[(int)severity]} [{tag}] {message}";

        var info = new LogMessageInfo {
            Message = logMessage,
            Severity = severity,
            Tag = tag,
            IgnoreFilter = ignoreFilter
        };

        logQueue.Enqueue(info);
    }

    struct LogMessageInfo
    {
        public string Message;
        public Severity Severity;
        public string Tag;
        public bool IgnoreFilter;
    }

    static void LogThread()
    {
        while (isLogging)
        {
            if (logQueue.TryDequeue(out var info))
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine(info.Message);
                }

                bool meetsWhitelist = whitelistTags.Count == 0 || whitelistTags.Contains(info.Tag);
                bool meetsBlacklist = blacklistTags.Count == 0 || !blacklistTags.Contains(info.Tag);
                bool meetsThreshold = info.IgnoreFilter || info.Severity >= Severity;
                if (meetsWhitelist && meetsBlacklist && meetsThreshold)
                {
                    if (info.Severity == Severity.Error || info.Severity == Severity.Fatal)
                    {
                        Console.Error.WriteLine(info.Message);
                    }
                    else
                    {
                        Console.WriteLine(info.Message);
                    }
                }
            }
            else
            {
                Thread.Sleep(10);
            }
        }
    }
}