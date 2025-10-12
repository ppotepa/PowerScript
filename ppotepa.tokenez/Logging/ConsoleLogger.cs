namespace ppotepa.tokenez.Logging
{
    /// <summary>
    /// Console-based logger implementation.
    /// Outputs colored messages to the console.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        public bool IsEnabled { get; set; } = true;

        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
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
                return;

            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = level switch
            {
                LogLevel.Debug => ConsoleColor.DarkGray,
                LogLevel.Info => ConsoleColor.Cyan,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Success => ConsoleColor.Green,
                _ => ConsoleColor.White
            };

            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }
    }
}
