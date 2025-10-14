namespace Tokenez.Common.Logging;

/// <summary>
///     Silent logger implementation that doesn't output anything.
///     Useful for testing or when logging should be suppressed.
/// </summary>
public class NullLogger : ILogger
{
    public bool IsEnabled { get; set; } = false;

    public void Debug(string message)
    {
    }

    public void Info(string message)
    {
    }

    public void Warning(string message)
    {
    }

    public void Error(string message)
    {
    }

    public void Success(string message)
    {
    }

    public void Log(LogLevel level, string message)
    {
    }
}