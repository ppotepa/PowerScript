namespace Tokenez.Common.Logging;

/// <summary>
///     Static logger service provider.
///     Provides global access to the logger instance.
/// </summary>
public static class LoggerService
{
    private static ILogger _logger = new ConsoleLogger();

    /// <summary>
    ///     Gets or sets the current logger implementation.
    /// </summary>
    public static ILogger Logger
    {
        get => _logger;
        set => _logger = value ?? new NullLogger();
    }

    /// <summary>
    ///     Sets the logger to a console logger.
    /// </summary>
    public static void UseConsoleLogger()
    {
        _logger = new ConsoleLogger();
    }

    /// <summary>
    ///     Sets the logger to a null logger (no output).
    /// </summary>
    public static void UseNullLogger()
    {
        _logger = new NullLogger();
    }

    /// <summary>
    ///     Enables or disables logging globally.
    /// </summary>
    public static void SetEnabled(bool enabled)
    {
        _logger.IsEnabled = enabled;
    }
}