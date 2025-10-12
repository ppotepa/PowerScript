#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ppotepa.tokenez.Interpreter;
using ppotepa.tokenez.Localization;
using ppotepa.tokenez.Logging;
using Serilog;
using Serilog.Events;

namespace ppotepa.tokenez
{
    /// <summary>
    ///     PowerScript Compiler - Simple compiler entry point
    ///     Takes code as string or filename and compiles/executes it.
    /// </summary>
    public static class Program
    {
        private static IServiceProvider? _serviceProvider;
        private static ILogger<PowerScriptInterpreter>? _logger;

        private static void Main(string[] args)
        {
            // Initialize Serilog with colorful console output
            InitializeSerilog();

            // Initialize IoC container
            InitializeServices();

            _logger = _serviceProvider!.GetRequiredService<ILogger<PowerScriptInterpreter>>();

            // Usage examples:
            // 1. Compile from file: ppotepa.tokenez.exe script.ps
            // 2. Compile inline code: (via code)

            if (args.Length > 0)
            {
                // Execute from file
                CompileAndExecuteFile(args[0]);
            }
            else
            {
                // Show usage information
                _logger.LogInformation("PowerScript Compiler v2.0");
                _logger.LogInformation("Usage: ppotepa.tokenez.exe <script.ps>");
                _logger.LogInformation("");
                _logger.LogInformation("Or use PowerScriptCompiler class in your code:");
                _logger.LogInformation("  var result = PowerScriptCompiler.CompileAndExecute(code);");
                _logger.LogInformation("  var result = PowerScriptCompiler.CompileAndExecuteFile(filename);");
            }

            // Cleanup
            Log.CloseAndFlush();
        }

        /// <summary>
        ///     Compiles and executes a PowerScript file
        /// </summary>
        public static object? CompileAndExecuteFile(string filename)
        {
            try
            {
                var interpreter = PowerScriptInterpreter.CreateNew();
                return interpreter.ExecuteFile(filename);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Execution error");
                throw;
            }
        }

        /// <summary>
        ///     Compiles and executes PowerScript code from a string
        /// </summary>
        public static object? CompileAndExecute(string code)
        {
            try
            {
                var interpreter = PowerScriptInterpreter.CreateNew();
                return interpreter.ExecuteCode(code);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Execution error");
                throw;
            }
        }

        /// <summary>
        ///     Initialize Serilog with colorful console output
        /// </summary>
        private static void InitializeSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
                .CreateLogger();

            Log.Information("PowerScript Compiler initialized with Serilog");
        }

        /// <summary>
        ///     Initialize the IoC container with all services
        /// </summary>
        private static void InitializeServices()
        {
            ServiceCollection services = new();

            // Add Serilog logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(Log.Logger, dispose: true);
            });

            // Add localization
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // Register custom logger service
            services.AddSingleton(typeof(Logging.ILogger), typeof(ConsoleLogger));

            // Build service provider
            _serviceProvider = services.BuildServiceProvider();

            // Initialize LoggerService with the custom logger
            var customLogger = _serviceProvider.GetRequiredService<Logging.ILogger>();
            LoggerService.Logger = customLogger;

            // Initialize localization
            var localizerFactory = _serviceProvider.GetRequiredService<IStringLocalizerFactory>();
            var localizer = localizerFactory.Create("Messages", typeof(Program).Assembly.GetName().Name!);
            LocalizationService.Initialize(localizer);
        }
    }
}