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
        private readonly List<string> _linkedLibraries = new List<string>();
        private string _linkedLibraryCode = null;

        /// <summary>
        /// Links a library file to be included with all script executions.
        /// </summary>
        /// <param name="libraryPath">Path to the library .ps file</param>
        public void LinkLibrary(string libraryPath)
        {
            string fullPath = ResolveFilePath(libraryPath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Library file not found: {fullPath}");
            }

            if (!_linkedLibraries.Contains(fullPath))
            {
                _linkedLibraries.Add(fullPath);
                // Reload library code
                ReloadLibraries();
            }
        }

        /// <summary>
        /// Reloads all linked libraries into a single code string.
        /// </summary>
        private void ReloadLibraries()
        {
            if (_linkedLibraries.Count == 0)
            {
                _linkedLibraryCode = null;
                return;
            }

            var libraryCode = new System.Text.StringBuilder();
            foreach (var libPath in _linkedLibraries)
            {
                libraryCode.AppendLine($"// Linked library: {Path.GetFileName(libPath)}");
                libraryCode.AppendLine(File.ReadAllText(libPath));
                libraryCode.AppendLine();
            }
            _linkedLibraryCode = libraryCode.ToString();
        }

        /// <summary>
        /// Executes PowerScript code directly from a string.
        /// </summary>
        /// <param name="code">The PowerScript source code to execute</param>
        /// <returns>Execution result (if any)</returns>
        public object ExecuteCode(string code)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[INTERPRETER] Executing PowerScript code...");
                Console.ResetColor();

                // Preprocess: expand LINK "file.ps" statements
                string expandedCode = ExpandLinkStatements(code);

                // Combine library code with script code
                string fullCode = expandedCode;
                if (_linkedLibraryCode != null)
                {
                    fullCode = _linkedLibraryCode + "\n" + expandedCode;
                }

                // Create prompt and build token tree
                UserPrompt prompt = new(fullCode, Array.Empty<string>());
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
        public object ExecuteFile(string filePath)
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
        /// Expands LINK "file.ps" statements by replacing them with the file content.
        /// Recursively processes nested LINK statements.
        /// </summary>
        private string ExpandLinkStatements(string code)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[LINK] Starting file expansion preprocessing...");
            Console.ResetColor();

            var linkedFiles = new HashSet<string>();  // Track to prevent circular references
            string result = ExpandLinkStatementsRecursive(code, linkedFiles);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[LINK] File expansion completed. Linked {linkedFiles.Count} file(s).");
            Console.ResetColor();

            return result;
        }

        private string ExpandLinkStatementsRecursive(string code, HashSet<string> linkedFiles)
        {
            var lines = code.Split('\n');
            var result = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // Check if this is a LINK statement with a file path (string literal)
                if (trimmed.StartsWith("LINK", StringComparison.OrdinalIgnoreCase) && trimmed.Contains("\""))
                {
                    try
                    {
                        // Extract the file path from the quotes
                        int firstQuote = trimmed.IndexOf('"');
                        int lastQuote = trimmed.LastIndexOf('"');

                        if (firstQuote >= 0 && lastQuote > firstQuote)
                        {
                            string filePath = trimmed.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
                            string resolvedPath = ResolveFilePath(filePath);

                            // Check for circular references
                            if (linkedFiles.Contains(resolvedPath))
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"[LINK] Skipping already linked file: {filePath}");
                                Console.ResetColor();
                                result.AppendLine($"// {line} (already linked)");
                                continue;
                            }

                            if (!File.Exists(resolvedPath))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"[LINK] File not found: {filePath} (resolved to: {resolvedPath})");
                                Console.ResetColor();
                                result.AppendLine(line); // Keep original line
                                continue;
                            }

                            linkedFiles.Add(resolvedPath);

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"[LINK] Expanding file: {filePath}");
                            Console.ResetColor();

                            // Read the file content
                            string fileContent = File.ReadAllText(resolvedPath);

                            // Recursively expand any LINK statements in the linked file
                            string expandedContent = ExpandLinkStatementsRecursive(fileContent, linkedFiles);

                            // Add a comment to show what was linked
                            result.AppendLine($"// === Linked from: {filePath} ===");
                            result.AppendLine(expandedContent);
                            result.AppendLine($"// === End of: {filePath} ===");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[LINK] Error processing LINK statement: {ex.Message}");
                        Console.ResetColor();
                        result.AppendLine(line); // Keep original line on error
                        continue;
                    }
                }

                // Not a file LINK statement, keep the line as-is
                result.AppendLine(line);
            }

            return result.ToString();
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
