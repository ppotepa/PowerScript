namespace ppotepa.tokenez.Logging
{
    /// <summary>
    /// Extension methods for convenient logger access.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Gets the current logger instance.
        /// </summary>
        public static ILogger Log => LoggerService.Logger;
    }
}
