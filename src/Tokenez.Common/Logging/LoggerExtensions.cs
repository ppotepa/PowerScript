using Tokenez.Common.Localization;

namespace Tokenez.Common.Logging
{
    /// <summary>
    ///     Extension methods for convenient logger access and localization support.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        ///     Gets the current logger instance.
        /// </summary>
        public static ILogger Log => LoggerService.Logger;

        /// <summary>
        ///     Logs a localized debug message.
        /// </summary>
        public static void DebugLocalized(this ILogger logger, string messageKey, params object[] args)
        {
#if DEBUG
            string message = LocalizationService.GetString(messageKey, args);
            logger.Debug(message);
#endif
        }

        /// <summary>
        ///     Logs a localized info message.
        /// </summary>
        public static void InfoLocalized(this ILogger logger, string messageKey, params object[] args)
        {
            string message = LocalizationService.GetString(messageKey, args);
            logger.Info(message);
        }

        /// <summary>
        ///     Logs a localized warning message.
        /// </summary>
        public static void WarningLocalized(this ILogger logger, string messageKey, params object[] args)
        {
            string message = LocalizationService.GetString(messageKey, args);
            logger.Warning(message);
        }

        /// <summary>
        ///     Logs a localized error message.
        /// </summary>
        public static void ErrorLocalized(this ILogger logger, string messageKey, params object[] args)
        {
            string message = LocalizationService.GetString(messageKey, args);
            logger.Error(message);
        }

        /// <summary>
        ///     Logs a localized success message.
        /// </summary>
        public static void SuccessLocalized(this ILogger logger, string messageKey, params object[] args)
        {
            string message = LocalizationService.GetString(messageKey, args);
            logger.Success(message);
        }

        /// <summary>
        ///     Writes a formatted header with a border.
        /// </summary>
        public static void WriteHeader(this ILogger? logger, string text, ConsoleColor color = ConsoleColor.Cyan)
        {
            if (logger != null)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ForegroundColor = originalColor;
            }
            else
            {
                throw new ArgumentNullException(nameof(logger));
            }
        }

        /// <summary>
        ///     Writes a separator line.
        /// </summary>
        public static void WriteSeparator(this ILogger _, ConsoleColor color = ConsoleColor.DarkGray)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(new string('─', 60));
            Console.ForegroundColor = originalColor;
        }
    }
}