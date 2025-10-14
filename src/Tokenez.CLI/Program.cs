#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Tokenez.Interpreter;
using Tokenez.Common.Localization;
using Tokenez.Common.Logging;
using Tokenez.Parser.Prompt;
using Tokenez.Parser.Lexer;
using Tokenez.Parser.Processors;
using Tokenez.Parser.Processors.Base;
using Tokenez.Compiler;
using Tokenez.Compiler.Interfaces;
using Tokenez.Core.DotNet;
using Tokenez.Interpreter.DotNet;
using Serilog;
using Serilog.Events;
using Tokenez.Interpreter.Interfaces;
using Tokenez.Parser.Processors.ControlFlow;
using Tokenez.Parser.Processors.Expressions;
using Tokenez.Parser.Processors.Scoping;
using Tokenez.Parser.Processors.Statements;

namespace Tokenez.CLI
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
            // 1. Compile from file: tokenez.exe script.ps
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
                _logger.LogInformation("Usage: tokenez.exe <script.ps>");
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
            services.AddSingleton(typeof(Tokenez.Common.Logging.ILogger), typeof(ConsoleLogger));

            // Register PowerScript core services
            RegisterPowerScriptServices(services);

            // Build service provider
            _serviceProvider = services.BuildServiceProvider();

            // Initialize the token processor registry with all processors
            var initAction = _serviceProvider.GetRequiredService<Action<IServiceProvider>>();
            initAction(_serviceProvider);

            // Initialize LoggerService with the custom logger
            var customLogger = _serviceProvider.GetRequiredService<Tokenez.Common.Logging.ILogger>();
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
            services.AddSingleton<IDotNetLinker, DotNetLinker>();

            // Step 1: Create empty registry (will be populated later)
            var registry = new TokenProcessorRegistry();
            services.AddSingleton<ITokenProcessorRegistry>(registry);

            // Step 2: Create ScopeBuilder with the registry
            services.AddSingleton<IScopeBuilder>(provider =>
            {
                var reg = provider.GetRequiredService<ITokenProcessorRegistry>();
                return new ScopeBuilder(reg);
            });

            // Step 3: Populate the registry with all processors (using a lazy factory pattern)
            services.AddSingleton<Action<IServiceProvider>>(provider =>
            {
                return (sp) =>
                {
                    var reg = sp.GetRequiredService<ITokenProcessorRegistry>();
                    var dotNetLinker = sp.GetRequiredService<IDotNetLinker>();
                    var scopeBuilder = sp.GetRequiredService<IScopeBuilder>();

                    // Create parameter processor (helper, not a token processor)
                    var parameterProcessor = new ParameterProcessor();

                    // Register all token processors
                    reg.Register(new FunctionProcessor(parameterProcessor));
                    reg.Register(new FunctionCallProcessor());
                    // TODO: LinkStatementProcessor not yet migrated to new structure
                    // reg.Register(new LinkStatementProcessor(dotNetLinker));
                    reg.Register(new FlexVariableProcessor());
                    reg.Register(new CycleLoopProcessor(scopeBuilder));
                    reg.Register(new IfStatementProcessor(scopeBuilder));
                    reg.Register(new ReturnStatementProcessor());
                    reg.Register(new PrintStatementProcessor());
                    reg.Register(new ExecuteCommandProcessor());
                    reg.Register(new NetMethodCallProcessor());
                    reg.Register(new VariableDeclarationProcessor());
                    reg.Register(new ScopeProcessor(reg, scopeBuilder));
                };
            });

            // Transient services (new instance per request)
            services.AddTransient<IPowerScriptInterpreter, PowerScriptInterpreter>();

            services.AddTransient<IPowerScriptCompiler>(provider =>
            {
                var reg = provider.GetRequiredService<ITokenProcessorRegistry>();
                var dotNetLinker = provider.GetRequiredService<IDotNetLinker>();
                var scopeBuilder = provider.GetRequiredService<IScopeBuilder>();
                var tokenTree = new TokenTree(reg, dotNetLinker, scopeBuilder);
                return new PowerScriptCompiler(tokenTree);
            });
            services.AddTransient<TokenTree>();
        }
    }
}