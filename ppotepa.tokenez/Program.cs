#nullable enable
using ppotepa.tokenez.Interpreter;
using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree;
using System.Text;

namespace ppotepa.tokenez
{
    /// <summary>
    /// PowerScript Interactive Interpreter (Shell)
    /// A PowerShell-like REPL environment for PowerScript language.
    /// Features:
    /// - Persistent interpreter session
    /// - Command history navigation
    /// - Multi-line input support
    /// - Script file execution
    /// - Built-in commands
    /// </summary>
    internal static class Program
    {
        private static List<string> _commandHistory = new List<string>();
        private static int _historyIndex = -1;
        private static PowerScriptInterpreter? _interpreter;

        static void Main(string[] args)
        {
            // Create persistent interpreter instance
            _interpreter = PowerScriptInterpreter.CreateNew();

            // Auto-link StdLib.ps if it exists
            // TEMPORARILY DISABLED FOR TESTING
            /*
            var stdLibPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs", "StdLib.ps");
            if (File.Exists(stdLibPath))
            {
                try
                {
                    _interpreter.LinkLibrary(stdLibPath);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[WARNING] Could not link StdLib.ps: {ex.Message}");
                    Console.ResetColor();
                }
            }
            */

            // Check execution mode
            if (args.Length > 0)
            {
                // Script execution mode
                ExecuteScriptMode(args[0]);
            }
            else
            {
                // Interactive shell mode (default)
                RunInteractiveShell();
            }
        }

        /// <summary>
        /// Executes a script file and exits
        /// </summary>
        static void ExecuteScriptMode(string scriptPath)
        {
            PrintBanner();
            Console.WriteLine($"\n[SCRIPT MODE] Executing: {scriptPath}\n");

            try
            {
                _interpreter!.ExecuteFile(scriptPath);
                Console.WriteLine("\n✓ Script execution completed successfully.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n✗ Script execution failed: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Runs the interactive PowerShell-like shell
        /// </summary>
        static void RunInteractiveShell()
        {
            PrintBanner();
            PrintWelcome();

            bool running = true;
            StringBuilder multiLineBuffer = new StringBuilder();
            bool inMultiLineMode = false;

            while (running)
            {
                try
                {
                    // Display prompt
                    if (inMultiLineMode)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(">> ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("PS> ");
                        Console.ResetColor();
                    }

                    // Read input
                    string? input = Console.ReadLine();

                    if (string.IsNullOrEmpty(input))
                    {
                        if (inMultiLineMode)
                        {
                            // Empty line in multi-line mode - execute accumulated code
                            ExecuteMultiLineCode(multiLineBuffer.ToString());
                            multiLineBuffer.Clear();
                            inMultiLineMode = false;
                        }
                        continue;
                    }

                    // Handle built-in commands
                    if (!inMultiLineMode && IsBuiltInCommand(input, ref running))
                        continue;

                    // Add to history
                    if (!inMultiLineMode)
                        AddToHistory(input);

                    // Check for multi-line indicators
                    if (!inMultiLineMode && (input.TrimEnd().EndsWith("{") || input.TrimStart().StartsWith("FUNCTION")))
                    {
                        inMultiLineMode = true;
                        multiLineBuffer.AppendLine(input);
                        continue;
                    }

                    // Multi-line mode
                    if (inMultiLineMode)
                    {
                        multiLineBuffer.AppendLine(input);

                        // Check if we should exit multi-line mode
                        if (input.TrimEnd().EndsWith("}"))
                        {
                            ExecuteMultiLineCode(multiLineBuffer.ToString());
                            multiLineBuffer.Clear();
                            inMultiLineMode = false;
                        }
                        continue;
                    }

                    // Single-line execution
                    ExecuteCommand(input);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nGoodbye!");
        }

        /// <summary>
        /// Handles built-in shell commands
        /// </summary>
        static bool IsBuiltInCommand(string input, ref bool running)
        {
            var command = input.Trim().ToUpper();

            switch (command)
            {
                case "EXIT":
                case "QUIT":
                    running = false;
                    return true;

                case "CLEAR":
                case "CLS":
                    Console.Clear();
                    PrintBanner();
                    return true;

                case "HELP":
                    ShowHelp();
                    return true;

                case "HISTORY":
                    ShowHistory();
                    return true;

                case "VERSION":
                    ShowVersion();
                    return true;

                case "ABOUT":
                    ShowAbout();
                    return true;

                default:
                    // Check for EXECUTE command
                    if (command.StartsWith("EXECUTE "))
                    {
                        string filePath = input.Substring(8).Trim().Trim('"');
                        try
                        {
                            _interpreter!.ExecuteFile(filePath);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Error: {ex.Message}");
                            Console.ResetColor();
                        }
                        return true;
                    }
                    return false;
            }
        }

        /// <summary>
        /// Executes a single-line command
        /// </summary>
        static void ExecuteCommand(string command)
        {
            try
            {
                _interpreter!.ExecuteCode(command);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Execution error: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Executes multi-line code block
        /// </summary>
        static void ExecuteMultiLineCode(string code)
        {
            try
            {
                _interpreter!.ExecuteCode(code);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Execution error: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Adds command to history
        /// </summary>
        static void AddToHistory(string command)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                _commandHistory.Add(command);
                _historyIndex = _commandHistory.Count;
            }
        }

        /// <summary>
        /// Prints the application banner
        /// </summary>
        static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              POWERSCRIPT INTERACTIVE SHELL v2.0                ║");
            Console.WriteLine("║          A .NET Wrapper Language Interpreter (REPL)            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        /// <summary>
        /// Prints welcome message and quick help
        /// </summary>
        static void PrintWelcome()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\nWelcome to PowerScript Interactive Shell!");
            Console.WriteLine("Type 'HELP' for available commands, 'EXIT' to quit.");
            Console.WriteLine("Enter PowerScript code directly at the prompt.\n");
            Console.ResetColor();

            // Display standard library summary
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Standard Library Loaded:");
            Console.WriteLine("  • Math: ADD, SUBTRACT, MULTIPLY, DIVIDE, MOD, POW");
            Console.WriteLine("  • String: CONCAT, LENGTH, SUBSTRING, TOUPPER, TOLOWER, TRIM");
            Console.WriteLine("  • I/O: PRINT, PRINTLN, READ");
            Console.WriteLine("  • .NET Access: NET::Namespace.Class.Method(args)\n");
            Console.ResetColor();
        }

        /// <summary>
        /// Shows help information
        /// </summary>
        static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    POWERSCRIPT HELP                          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            Console.WriteLine("Built-in Shell Commands:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  HELP            Show this help message");
            Console.WriteLine("  EXIT, QUIT      Exit the interpreter");
            Console.WriteLine("  CLEAR, CLS      Clear the screen");
            Console.WriteLine("  HISTORY         Show command history");
            Console.WriteLine("  VERSION         Show version information");
            Console.WriteLine("  ABOUT           About PowerScript");
            Console.ResetColor();

            Console.WriteLine("\nPowerScript Syntax:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  PRINT \"text\"                    Print output");
            Console.WriteLine("  VAR name = value                 Declare variable");
            Console.WriteLine("  VAR INT name = value             Declare typed variable");
            Console.WriteLine("  NET::System.Console.WriteLine()  Call .NET methods");
            Console.WriteLine("  EXECUTE \"file.ps\"                Execute script file");
            Console.WriteLine("  FUNCTION name(params) { ... }    Define function");
            Console.WriteLine("  RETURN expression                Return from function");
            Console.ResetColor();

            Console.WriteLine("\nExamples:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  PS> PRINT \"Hello, World!\"");
            Console.WriteLine("  PS> VAR x = 10");
            Console.WriteLine("  PS> VAR INT count = 5");
            Console.WriteLine("  PS> NET::System.Console.WriteLine(\"From .NET!\")");
            Console.WriteLine("  PS> EXECUTE \"myscript.ps\"");
            Console.WriteLine("  PS> FUNCTION ADD(INT a, INT b) { RETURN a + b }");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Shows command history
        /// </summary>
        static void ShowHistory()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nCommand History:");
            Console.ResetColor();

            if (_commandHistory.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  (empty)");
                Console.ResetColor();
            }
            else
            {
                for (int i = 0; i < _commandHistory.Count; i++)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"  [{i + 1}] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(_commandHistory[i]);
                    Console.ResetColor();
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Shows version information
        /// </summary>
        static void ShowVersion()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nPowerScript Interpreter v2.0");
            Console.WriteLine("Build Date: October 2025");
            Console.WriteLine("Runtime: .NET 8.0");
            Console.WriteLine("Language: PowerScript");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\nFeatures:");
            Console.WriteLine("  ✓ Interactive REPL Shell");
            Console.WriteLine("  ✓ Script File Execution");
            Console.WriteLine("  ✓ .NET Framework Integration");
            Console.WriteLine("  ✓ Function Compilation");
            Console.WriteLine("  ✓ Multi-line Input Support");
            Console.WriteLine("  ✓ Command History");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Shows about information
        /// </summary>
        static void ShowAbout()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                  ABOUT POWERSCRIPT                           ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nPowerScript is a .NET wrapper language with an interactive");
            Console.WriteLine("interpreter shell. It provides direct access to the .NET");
            Console.WriteLine("Framework through a simple, PowerShell-like syntax.");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nKey Features:");
            Console.WriteLine("  • Direct .NET method invocation via NET:: syntax");
            Console.WriteLine("  • Function compilation to Lambda expressions");
            Console.WriteLine("  • Script file execution and imports");
            Console.WriteLine("  • Interactive REPL for rapid development");
            Console.WriteLine("  • PowerShell-like command interface");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\nArchitecture:");
            Console.WriteLine("  • Token-based lexical analysis");
            Console.WriteLine("  • AST construction and traversal");
            Console.WriteLine("  • Reflection-based .NET integration");
            Console.WriteLine("  • Expression tree compilation");
            Console.WriteLine("  • Persistent interpreter session");

            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
