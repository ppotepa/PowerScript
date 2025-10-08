using ppotepa.tokenez.Interpreter;
using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.StandardLib;
using ppotepa.tokenez.Tree;

namespace ppotepa.tokenez
{
    /// <summary>
    /// Main entry point for PowerScript Interpreter.
    /// Can run in three modes:
    /// 1. Interactive REPL mode (no arguments)
    /// 2. Execute script file (provide file path as argument)
    /// 3. Legacy mode (read program.ps from current directory)
    /// </summary>
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                POWERSCRIPT INTERPRETER v2.0                    ║");
            Console.WriteLine("║          A .NET Wrapper Language with Script Execution         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

            // Display the standard library
            var standardLibrary = new StandardLibrary();
            standardLibrary.DisplayFunctions();

            // Create the interpreter
            var interpreter = PowerScriptInterpreter.CreateNew();

            // Check if a script file was provided as a command line argument
            if (args.Length > 0)
            {
                // Mode 1: Execute the provided script file
                string scriptPath = args[0];
                Console.WriteLine($"\n[MODE] Executing script from command line: {scriptPath}\n");
                interpreter.ExecuteFile(scriptPath);
                Console.WriteLine("\n╔════════════════════════════════════════╗");
                Console.WriteLine("║        EXECUTION COMPLETE              ║");
                Console.WriteLine("╚════════════════════════════════════════╝\n");
            }
            else if (File.Exists("program.ps"))
            {
                // Mode 2: Legacy mode - read program.ps from current directory
                Console.WriteLine("\n[MODE] Legacy mode - executing program.ps from current directory\n");
                interpreter.ExecuteFile("program.ps");
                Console.WriteLine("\n╔════════════════════════════════════════╗");
                Console.WriteLine("║        EXECUTION COMPLETE              ║");
                Console.WriteLine("╚════════════════════════════════════════╝\n");
            }
            else
            {
                // Mode 3: Interactive REPL mode
                Console.WriteLine("\n[MODE] Interactive REPL mode");
                Console.WriteLine("Commands:");
                Console.WriteLine("  - Type PowerScript code to execute");
                Console.WriteLine("  - EXECUTE \"filename.ps\" to run a script file");
                Console.WriteLine("  - EXIT to quit\n");
                RunInteractiveMode(interpreter);
            }

            Console.WriteLine("Program execution complete.");
        }

        /// <summary>
        /// Runs the interactive Read-Eval-Print Loop (REPL) mode.
        /// </summary>
        static void RunInteractiveMode(PowerScriptInterpreter interpreter)
        {
            bool running = true;
            while (running)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("ps> ");
                Console.ResetColor();
                
                string? command = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(command))
                    continue;

                if (command.Equals("EXIT", StringComparison.OrdinalIgnoreCase) ||
                    command.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    running = false;
                    Console.WriteLine("\nExiting PowerScript interpreter. Goodbye!");
                    continue;
                }

                try
                {
                    // Check if it's an EXECUTE command for direct file execution
                    if (command.StartsWith("EXECUTE ", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract file path from command
                        string filePath = command.Substring(8).Trim();
                        // Remove quotes if present
                        if (filePath.StartsWith("\"") && filePath.EndsWith("\""))
                            filePath = filePath.Substring(1, filePath.Length - 2);
                        
                        interpreter.ExecuteFile(filePath);
                    }
                    else
                    {
                        // Normal code execution
                        interpreter.ExecuteCode(command);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }
    }
}
