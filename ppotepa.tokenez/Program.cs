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
                var interpreter = _serviceProvider!.GetRequiredService<IPowerScriptInterpreter>();
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
                var interpreter = _serviceProvider!.GetRequiredService<IPowerScriptInterpreter>();
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

            // Register PowerScript core services
            RegisterPowerScriptServices(services);

            // Build service provider
            _serviceProvider = services.BuildServiceProvider();

            // Initialize the token processor registry with all processors
            var initAction = _serviceProvider.GetRequiredService<Action<IServiceProvider>>();
            initAction(_serviceProvider);

            // Initialize LoggerService with the custom logger
            var customLogger = _serviceProvider.GetRequiredService<Logging.ILogger>();
            LoggerService.Logger = customLogger;

            // Initialize localization
            var localizerFactory = _serviceProvider.GetRequiredService<IStringLocalizerFactory>();
            var localizer = localizerFactory.Create("Messages", typeof(Program).Assembly.GetName().Name!);
            LocalizationService.Initialize(localizer);
        }

        /// <summary>
        ///     Registers all PowerScript-specific services in the DI container
        /// </summary>
        private static void RegisterPowerScriptServices(ServiceCollection services)
        {
            // Core infrastructure - Singletons (shared across application)
            services.AddSingleton<DotNet.IDotNetLinker, DotNet.DotNetLinker>();

            // Step 1: Create empty registry (will be populated later)
            var registry = new Tree.Builders.TokenProcessorRegistry();
            services.AddSingleton<Tree.Builders.ITokenProcessorRegistry>(registry);

            // Step 2: Create ScopeBuilder with the registry
            services.AddSingleton<Tree.Builders.IScopeBuilder>(provider =>
            {
                var reg = provider.GetRequiredService<Tree.Builders.ITokenProcessorRegistry>();
                return new Tree.Builders.ScopeBuilder(reg);
            });

            // Step 3: Populate the registry with all processors (using a lazy factory pattern)
            services.AddSingleton<Action<IServiceProvider>>(provider =>
            {
                return (sp) =>
                {
                    var reg = sp.GetRequiredService<Tree.Builders.ITokenProcessorRegistry>();
                    var dotNetLinker = sp.GetRequiredService<DotNet.IDotNetLinker>();
                    var scopeBuilder = sp.GetRequiredService<Tree.Builders.IScopeBuilder>();

                    // Create parameter processor (helper, not a token processor)
                    var parameterProcessor = new Tree.Builders.ParameterProcessor();

                    // Register all token processors
                    reg.Register(new Tree.Builders.FunctionProcessor(parameterProcessor));
                    reg.Register(new Tree.Builders.FunctionCallProcessor());
                    reg.Register(new Tree.Builders.LinkStatementProcessor(dotNetLinker));
                    reg.Register(new Tree.Builders.FlexVariableProcessor());
                    reg.Register(new Tree.Builders.CycleLoopProcessor(scopeBuilder));
                    reg.Register(new Tree.Builders.IfStatementProcessor(scopeBuilder));
                    reg.Register(new Tree.Builders.ReturnStatementProcessor());
                    reg.Register(new Tree.Builders.PrintStatementProcessor());
                    reg.Register(new Tree.Builders.ExecuteCommandProcessor());
                    reg.Register(new Tree.Builders.NetMethodCallProcessor());
                    reg.Register(new Tree.Builders.VariableDeclarationProcessor());
                    reg.Register(new Tree.Builders.ScopeProcessor(reg, scopeBuilder));
                };
            });

            // Transient services (new instance per request)
            services.AddTransient<IPowerScriptInterpreter, PowerScriptInterpreter>();

            services.AddTransient<Tree.IPowerScriptCompiler>(provider =>
            {
                var reg = provider.GetRequiredService<Tree.Builders.ITokenProcessorRegistry>();
                var dotNetLinker = provider.GetRequiredService<DotNet.IDotNetLinker>();
                var scopeBuilder = provider.GetRequiredService<Tree.Builders.IScopeBuilder>();
                var tokenTree = new Tree.TokenTree(reg, dotNetLinker, scopeBuilder);
                return new Tree.PowerScriptCompiler(tokenTree);
            });
            services.AddTransient<Tree.TokenTree>();
        }
    }
}