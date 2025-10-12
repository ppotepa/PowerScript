#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using ppotepa.tokenez.Interpreter;
using ppotepa.tokenez.Localization;

namespace ppotepa.tokenez
{
    /// <summary>
    ///     PowerScript Compiler - Simple compiler entry point
    ///     Takes code as string or filename and compiles/executes it.
    /// </summary>
    public static class Program
    {
        private static void Main(string[] args)
        {
            // Initialize localization
            InitializeLocalization();

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
                // Example: Compile from string
                Console.WriteLine("PowerScript Compiler v2.0");
                Console.WriteLine("Usage: ppotepa.tokenez.exe <script.ps>");
                Console.WriteLine("\nOr use PowerScriptCompiler class in your code:");
                Console.WriteLine("  var result = PowerScriptCompiler.CompileAndExecute(code);");
                Console.WriteLine("  var result = PowerScriptCompiler.CompileAndExecuteFile(filename);");
            }
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {ex.Message}");
                Console.ResetColor();
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }

        /// <summary>
        ///     Initialize the localization system
        /// </summary>
        private static void InitializeLocalization()
        {
            ServiceCollection services = new();
            services.AddLogging();
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            var serviceProvider = services.BuildServiceProvider();
            var localizerFactory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();
            var localizer = localizerFactory.Create("Messages", typeof(Program).Assembly.GetName().Name!);
            LocalizationService.Initialize(localizer);
        }
    }
}