using PowerScript.Common.Localization;

namespace PowerScript.Common.Logging;

/// <summary>
///     Console-based logger implementation with localization support.
///     Outputs colored messages to the console with support for DEBUG/RELEASE modes.
/// </summary>
public class ConsoleLogger : ILogger
{
    public bool IsEnabled { get; set; } = true;

    public void Debug(string message)
    {
#if DEBUG
        Log(LogLevel.Debug, message);
#endif
    }

    public void Info(string message)
    {
        Log(LogLevel.Info, message);
    }

    public void Warning(string message)
    {
        Log(LogLevel.Warning, message);
    }

    public void Error(string message)
    {
        Log(LogLevel.Error, message);
    }

    public void Success(string message)
    {
        Log(LogLevel.Success, message);
    }

    public void Log(LogLevel level, string message)
    {
        if (!IsEnabled)
        {
            return;
        }

        ConsoleColor originalColor = Console.ForegroundColor;

        // Set color based on log level
        Console.ForegroundColor = level switch
        {
            LogLevel.Debug => ConsoleColor.DarkGray,
            LogLevel.Info => ConsoleColor.Cyan,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Success => ConsoleColor.Green,
            _ => ConsoleColor.White
        };

        // Add prefix for debug builds
#if DEBUG
        string prefix = level switch
        {
            LogLevel.Debug => "[DEBUG] ",
            LogLevel.Info => "[INFO] ",
            LogLevel.Warning => "[WARN] ",
            LogLevel.Error => "[ERROR] ",
            LogLevel.Success => "[OK] ",
            _ => ""
        };
        // Write debug logs to stderr to avoid polluting program output
        if (level == LogLevel.Debug)
        {
            Console.Error.WriteLine(prefix + message);
        }
        else
        {
            Console.WriteLine(prefix + message);
        }
#else
                Console.WriteLine(message);
#endif

        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    ///     Logs a localized message by key.
    /// </summary>
    public void LogLocalized(LogLevel level, string messageKey)
    {
        string message = LocalizationService.GetString(messageKey);
        Log(level, message);
    }

    /// <summary>
    ///     Logs a localized message by key with parameters.
    /// </summary>
    public void LogLocalized(LogLevel level, string messageKey, params object[] args)
    {
        string message = LocalizationService.GetString(messageKey, args);
        Log(level, message);
    }

    /// <summary>
    ///     Writes a colorful header box.
    /// </summary>
    public void WriteHeader(string title, ConsoleColor color = ConsoleColor.Cyan)
    {
        if (!IsEnabled)
        {
            return;
        }

        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;

        int width = Math.Max(title.Length + 4, 60);
        string line = new('═', width);
        int padding = (width - title.Length - 2) / 2;
        string titleLine = "║" + new string(' ', padding) + title + new string(' ', width - padding - title.Length - 2) +
                        "║";

        Console.WriteLine("╔" + line + "╗");
        Console.WriteLine(titleLine);
        Console.WriteLine("╚" + line + "╝");

        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    ///     Writes a section separator.
    /// </summary>
    public void WriteSeparator(ConsoleColor color = ConsoleColor.DarkGray)
    {
        if (!IsEnabled)
        {
            return;
        }

        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(new string('─', 60));
        Console.ForegroundColor = originalColor;
    }
}