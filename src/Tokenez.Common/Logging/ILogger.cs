namespace Tokenez.Common.Logging
{
    /// <summary>
    ///     Interface for logging services.
    ///     Allows interchangeable logger implementations.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        ///     Gets or sets whether logging is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        ///     Logs a debug message (gray color in console).
        /// </summary>
        void Debug(string message);

        /// <summary>
        ///     Logs an informational message (cyan color in console).
        /// </summary>
        void Info(string message);

        /// <summary>
        ///     Logs a warning message (yellow color in console).
        /// </summary>
        void Warning(string message);

        /// <summary>
        ///     Logs an error message (red color in console).
        /// </summary>
        void Error(string message);

        /// <summary>
        ///     Logs a success message (green color in console).
        /// </summary>
        void Success(string message);

        /// <summary>
        ///     Logs a message with specified log level.
        /// </summary>
        void Log(LogLevel level, string message);
    }
}