using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PowerScript.Common.Localization;
using PowerScript.Common.Logging;
using PowerScript.Compiler;
using PowerScript.Compiler.Interfaces;
using PowerScript.Core.DotNet;
using PowerScript.Core.Syntax;
using PowerScript.Interpreter;
using PowerScript.Interpreter.DotNet;
using PowerScript.Interpreter.Interfaces;
using PowerScript.Parser.Lexer;
using PowerScript.Parser.Processors.Base;
using PowerScript.Parser.Processors.ControlFlow;
using PowerScript.Parser.Processors.Expressions;
using PowerScript.Parser.Processors.Scoping;
using PowerScript.Parser.Processors.Statements;
using PowerScript.Runtime;
using PowerScript.Runtime.Interfaces;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Logging_ILogger = PowerScript.Common.Logging.ILogger;

namespace PowerScript.CLI;

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
        // 1. Compile from file: powerscript.exe script.ps
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
            _logger.LogInformation("Usage: powerscript.exe <script.ps>");
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
            IPowerScriptInterpreter interpreter = _serviceProvider!.GetRequiredService<IPowerScriptInterpreter>();
            return interpreter.ExecuteFile(filename);
        }
        catch (Exception ex)
        {
            _logger?.LogError("Execution error: {Message}", ex.Message);
            throw; // rethrow without logging exception object to avoid duplicate stack traces
        }
    }

    /// <summary>
    ///     Compiles and executes PowerScript code from a string
    /// </summary>
    public static object? CompileAndExecute(string code)
    {
        try
        {
            IPowerScriptInterpreter interpreter = _serviceProvider!.GetRequiredService<IPowerScriptInterpreter>();
            return interpreter.ExecuteCode(code);
        }
        catch (Exception ex)
        {
            _logger?.LogError("Execution error: {Message}", ex.Message);
            throw; // rethrow without logging exception object to avoid duplicate stack traces
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
                theme: AnsiConsoleTheme.Code)
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
            builder.AddSerilog(Log.Logger, true);
        });

        // Add localization
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Register custom logger service
        services.AddSingleton(typeof(Logging_ILogger), typeof(ConsoleLogger));

        // Register PowerScript core services
        RegisterPowerScriptServices(services);

        // Build service provider
        _serviceProvider = services.BuildServiceProvider();

        // Initialize the token processor registry with all processors
        Action<IServiceProvider> initAction = _serviceProvider.GetRequiredService<Action<IServiceProvider>>();
        initAction(_serviceProvider);

        // Initialize custom syntax from .psx files
        InitializeCustomSyntax();

        // Initialize LoggerService with the custom logger
        Logging_ILogger customLogger = _serviceProvider.GetRequiredService<Logging_ILogger>();
        LoggerService.Logger = customLogger;

        // Initialize localization
        IStringLocalizerFactory localizerFactory = _serviceProvider.GetRequiredService<IStringLocalizerFactory>();
        IStringLocalizer localizer = localizerFactory.Create("Messages", typeof(Program).Assembly.GetName().Name!);
        LocalizationService.Initialize(localizer);
    }

    /// <summary>
    ///     Initializes custom syntax by loading .psx files from stdlib/syntax directory
    /// </summary>
    private static void InitializeCustomSyntax()
    {
        try
        {
            // Try multiple starting points to find stdlib
            string? stdlibPath = null;

            // 1. Try from current working directory (works with dotnet run)
            stdlibPath = FindStdlibDirectory(Directory.GetCurrentDirectory());

            // 2. If not found, try from base directory (works with compiled exe)
            if (stdlibPath == null)
            {
                stdlibPath = FindStdlibDirectory(AppDomain.CurrentDomain.BaseDirectory);
            }

            if (stdlibPath != null)
            {
                PsxFileLoader.LoadStandardSyntax(stdlibPath);
                Log.Information($"Custom syntax loaded from: {Path.Combine(stdlibPath, "syntax")}");
            }
            else
            {
                Log.Warning("stdlib directory not found - custom syntax will not be available");
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Failed to load custom syntax: {ex.Message}");
        }
    }

    /// <summary>
    ///     Searches for the stdlib directory starting from the given path
    /// </summary>
    private static string? FindStdlibDirectory(string startPath)
    {
        string currentPath = startPath;

        // Try current directory and up to 5 levels up
        for (int i = 0; i < 6; i++)
        {
            string stdlibPath = Path.Combine(currentPath, "stdlib");
            if (Directory.Exists(stdlibPath))
            {
                return stdlibPath;
            }

            string? parentPath = Directory.GetParent(currentPath)?.FullName;
            if (parentPath == null)
            {
                break;
            }
            currentPath = parentPath;
        }

        return null;
    }

    /// <summary>
    ///     Registers all PowerScript-specific services in the DI container
    /// </summary>
    private static void RegisterPowerScriptServices(ServiceCollection services)
    {
        // Core infrastructure - Singletons (shared across application)
        services.AddSingleton<IDotNetLinker, DotNetLinker>();

        // Step 1: Create empty registry (will be populated later)
        TokenProcessorRegistry registry = new();
        services.AddSingleton<ITokenProcessorRegistry>(registry);

        // Step 2: Create ScopeBuilder with the registry
        services.AddSingleton<IScopeBuilder>(provider =>
        {
            ITokenProcessorRegistry reg = provider.GetRequiredService<ITokenProcessorRegistry>();
            return new ScopeBuilder(reg);
        });

        // Step 3: Populate the registry with all processors (using a lazy factory pattern)
        services.AddSingleton<Action<IServiceProvider>>(provider =>
        {
            return sp =>
            {
                ITokenProcessorRegistry reg = sp.GetRequiredService<ITokenProcessorRegistry>();
                IDotNetLinker dotNetLinker = sp.GetRequiredService<IDotNetLinker>();
                IScopeBuilder scopeBuilder = sp.GetRequiredService<IScopeBuilder>();

                // Create parameter processor (helper, not a token processor)
                ParameterProcessor parameterProcessor = new();

                // Register all token processors
                // Order matters: more specific processors should come first
                reg.Register(new PatternSyntaxProcessor()); // FILTER ... WHERE ... (custom syntax patterns)
                reg.Register(new StaticTypeVariableProcessor()); // INT, STRING, NUMBER
                reg.Register(new FunctionProcessor(parameterProcessor));
                reg.Register(new NetMemberAccessStatementProcessor()); // #Console -> WriteLine(42)
                reg.Register(new LinkStatementProcessor()); // LINK System or LINK "file.ps"
                reg.Register(new FlexVariableProcessor());
                reg.Register(new StaticTypeVariableProcessor()); // INT, STRING, NUMBER
                reg.Register(new VariableAssignmentProcessor()); // identifier = value
                reg.Register(new CycleLoopProcessor(scopeBuilder));
                reg.Register(new IfStatementProcessor(scopeBuilder));
                reg.Register(new ReturnStatementProcessor());
                // PrintStatementProcessor removed - PRINT is now a library function
                reg.Register(new FunctionCallStatementProcessor()); // Single-param: PRINT x, Multi-param: FUNC(a,b) - MUST come before FunctionCallProcessor
                reg.Register(new FunctionCallProcessor()); // Legacy processor - kept for backwards compatibility
                reg.Register(new ExecuteCommandProcessor());
                reg.Register(new NetMethodCallProcessor());
                reg.Register(new VariableDeclarationProcessor()); // VAR
                // NOTE: ScopeProcessor creates circular reference - ScopeBuilder handles scopes internally
                // reg.Register(new ScopeProcessor(reg, scopeBuilder));
            };
        });

        // Transient services (new instance per request)

        // Register the new separated domain components
        services.AddTransient<IPowerScriptCompilerNew>(provider =>
        {
            ITokenProcessorRegistry registry = provider.GetRequiredService<ITokenProcessorRegistry>();
            IDotNetLinker dotNetLinker = provider.GetRequiredService<IDotNetLinker>();
            IScopeBuilder scopeBuilder = provider.GetRequiredService<IScopeBuilder>();
            return new PowerScriptCompilerNew(registry, dotNetLinker, scopeBuilder);
        });

        services.AddTransient<IPowerScriptExecutor, PowerScriptExecutor>();

        services.AddTransient<IPowerScriptInterpreter>(provider =>
        {
            IPowerScriptCompilerNew compiler = provider.GetRequiredService<IPowerScriptCompilerNew>();
            IPowerScriptExecutor executor = provider.GetRequiredService<IPowerScriptExecutor>();
            return new PowerScriptInterpreter(compiler, executor);
        });
        services.AddTransient<TokenTree>();
    }
}