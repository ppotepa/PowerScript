using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree;

namespace ppotepa.tokenez.Interpreter
{
    /// <summary>
    /// Core interpreter for PowerScript language that handles script execution.
    /// Can execute code from strings or files, maintaining a global execution context.
    /// </summary>
    public class PowerScriptInterpreter
    {
        private readonly Dictionary<string, TokenTree> _compiledScripts = new Dictionary<string, TokenTree>();

        /// <summary>
        /// Executes PowerScript code directly from a string.
        /// </summary>
        /// <param name="code">The PowerScript source code to execute</param>
        /// <returns>Execution result (if any)</returns>
        public object? ExecuteCode(string code)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[INTERPRETER] Executing PowerScript code...");
                Console.ResetColor();

                // Create prompt and build token tree
                UserPrompt prompt = new(code, Array.Empty<string>());
                TokenTree tree = new TokenTree().Create(prompt);

                // Compile and execute
                PowerScriptCompiler compiler = new(tree);
                compiler.CompileAndExecute();

                return null;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[INTERPRETER] Execution error: {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }

        /// <summary>
        /// Executes a PowerScript file.
        /// </summary>
        /// <param name="filePath">Path to the .ps script file</param>
        /// <returns>Execution result (if any)</returns>
        public object? ExecuteFile(string filePath)
        {
            try
            {
                // Resolve the file path
                string fullPath = ResolveFilePath(filePath);

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"Script file not found: {fullPath}");
                }

                // Read the file content
                string code = File.ReadAllText(fullPath);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n╔════════════════════════════════════════╗");
                Console.WriteLine($"║  EXECUTING SCRIPT: {Path.GetFileName(fullPath),-20} ║");
                Console.WriteLine($"╚════════════════════════════════════════╝");
                Console.ResetColor();

                // Execute the code
                return ExecuteCode(code);
            }
            catch (FileNotFoundException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[INTERPRETER] File not found: {ex.Message}");
                Console.ResetColor();
                return null;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[INTERPRETER] Error executing file {filePath}: {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }

        /// <summary>
        /// Resolves a file path, checking current directory and relative paths.
        /// </summary>
        private string ResolveFilePath(string filePath)
        {
            // If the path is already absolute and exists, return it
            if (Path.IsPathRooted(filePath) && File.Exists(filePath))
            {
                return filePath;
            }

            // Try relative to current directory
            string currentDirPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            if (File.Exists(currentDirPath))
            {
                return currentDirPath;
            }

            // Return the original path (will fail if not found)
            return filePath;
        }

        /// <summary>
        /// Creates a new interpreter instance.
        /// </summary>
        public static PowerScriptInterpreter CreateNew()
        {
            return new PowerScriptInterpreter();
        }
    }
}
