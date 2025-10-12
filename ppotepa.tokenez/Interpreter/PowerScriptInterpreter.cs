using System.Text;
using ppotepa.tokenez.Logging;
using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree;

namespace ppotepa.tokenez.Interpreter
{
    /// <summary>
    ///     Core interpreter for PowerScript language that handles script execution.
    ///     Can execute code from strings or files, maintaining a global execution context.
    /// </summary>
    public class PowerScriptInterpreter
    {
        private readonly Dictionary<string, TokenTree> _compiledScripts = [];
        private readonly List<string> _linkedLibraries = [];
        private string? _linkedLibraryCode;

        /// <summary>
        ///     Links a library file to be included with all script executions.
        /// </summary>
        /// <param name="libraryPath">Path to the library .ps file</param>
        public void LinkLibrary(string libraryPath)
        {
            var fullPath = ResolveFilePath(libraryPath);

            if (!File.Exists(fullPath)) throw new FileNotFoundException($"Library file not found: {fullPath}");

            if (!_linkedLibraries.Contains(fullPath))
            {
                _linkedLibraries.Add(fullPath);
                // Reload library code
                ReloadLibraries();
            }
        }

        /// <summary>
        ///     Reloads all linked libraries into a single code string.
        /// </summary>
        private void ReloadLibraries()
        {
            if (_linkedLibraries.Count == 0)
            {
                _linkedLibraryCode = null;
                return;
            }

            StringBuilder libraryCode = new();
            foreach (var libPath in _linkedLibraries)
            {
                libraryCode.AppendLine($"// Linked library: {Path.GetFileName(libPath)}");
                libraryCode.AppendLine(File.ReadAllText(libPath));
                libraryCode.AppendLine();
            }

            _linkedLibraryCode = libraryCode.ToString();
        }

        /// <summary>
        ///     Executes PowerScript code directly from a string.
        /// </summary>
        /// <param name="code">The PowerScript source code to execute</param>
        /// <returns>Execution result (if any)</returns>
        public object? ExecuteCode(string code)
        {
            try
            {
                LoggerService.Logger.Info("Executing PowerScript code...");

                // Preprocess: expand LINK "file.ps" statements
                var expandedCode = ExpandLinkStatements(code);

                // Combine library code with script code
                var fullCode = expandedCode;
                if (_linkedLibraryCode != null) fullCode = _linkedLibraryCode + "\n" + expandedCode;

                // Create prompt and build token tree
                UserPrompt prompt = new(fullCode);
                var tree = new TokenTree().Create(prompt);

                // Compile and execute
                PowerScriptCompiler compiler = new(tree);
                compiler.CompileAndExecute();

                return null;
            }
            catch (Exception ex)
            {
                LoggerService.Logger.Error($"Execution error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        ///     Executes a PowerScript file.
        /// </summary>
        /// <param name="filePath">Path to the .ps script file</param>
        /// <returns>Execution result (if any)</returns>
        public object? ExecuteFile(string filePath)
        {
            try
            {
                // Resolve the file path
                var fullPath = ResolveFilePath(filePath);

                if (!File.Exists(fullPath)) throw new FileNotFoundException($"Script file not found: {fullPath}");

                // Read the file content
                var code = File.ReadAllText(fullPath);

                LoggerService.Logger.Info("");
                LoggerService.Logger.Info("╔════════════════════════════════════════╗");
                LoggerService.Logger.Info($"║  EXECUTING SCRIPT: {Path.GetFileName(fullPath),-20} ║");
                LoggerService.Logger.Info("╚════════════════════════════════════════╝");

                // Execute the code
                return ExecuteCode(code);
            }
            catch (FileNotFoundException ex)
            {
                LoggerService.Logger.Error($"File not found: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                LoggerService.Logger.Error($"Error executing file {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        ///     Resolves a file path, checking current directory and relative paths.
        /// </summary>
        private string ResolveFilePath(string filePath)
        {
            // If the path is already absolute and exists, return it
            if (Path.IsPathRooted(filePath) && File.Exists(filePath)) return filePath;

            // Try relative to current directory
            var currentDirPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            if (File.Exists(currentDirPath)) return currentDirPath;

            // Return the original path (will fail if not found)
            return filePath;
        }

        /// <summary>
        ///     Expands LINK "file.ps" statements by replacing them with the file content.
        ///     Recursively processes nested LINK statements.
        /// </summary>
        private string ExpandLinkStatements(string code)
        {
            LoggerService.Logger.Debug("Starting file expansion preprocessing...");

            HashSet<string> linkedFiles = []; // Track to prevent circular references
            var result = ExpandLinkStatementsRecursive(code, linkedFiles);

            LoggerService.Logger.Debug($"File expansion completed. Linked {linkedFiles.Count} file(s).");

            return result;
        }

        private string ExpandLinkStatementsRecursive(string code, HashSet<string> linkedFiles)
        {
            var lines = code.Split('\n');
            StringBuilder result = new();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // Check if this is a LINK statement with a file path (string literal)
                if (trimmed.StartsWith("LINK", StringComparison.OrdinalIgnoreCase) && trimmed.Contains("\""))
                    try
                    {
                        // Extract the file path from the quotes
                        var firstQuote = trimmed.IndexOf('"');
                        var lastQuote = trimmed.LastIndexOf('"');

                        if (firstQuote >= 0 && lastQuote > firstQuote)
                        {
                            var filePath = trimmed.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
                            var resolvedPath = ResolveFilePath(filePath);

                            // Check for circular references
                            if (linkedFiles.Contains(resolvedPath))
                            {
                                LoggerService.Logger.Warning($"Skipping already linked file: {filePath}");
                                result.AppendLine($"// {line} (already linked)");
                                continue;
                            }

                            if (!File.Exists(resolvedPath))
                            {
                                LoggerService.Logger.Error($"File not found: {filePath} (resolved to: {resolvedPath})");
                                result.AppendLine(line); // Keep original line
                                continue;
                            }

                            linkedFiles.Add(resolvedPath);

                            LoggerService.Logger.Success($"Expanding file: {filePath}");

                            // Read the file content
                            var fileContent = File.ReadAllText(resolvedPath);

                            // Recursively expand any LINK statements in the linked file
                            var expandedContent = ExpandLinkStatementsRecursive(fileContent, linkedFiles);

                            // Add a comment to show what was linked
                            result.AppendLine($"// === Linked from: {filePath} ===");
                            result.AppendLine(expandedContent);
                            result.AppendLine($"// === End of: {filePath} ===");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerService.Logger.Error($"Error processing LINK statement: {ex.Message}");
                        result.AppendLine(line); // Keep original line on error
                        continue;
                    }

                // Not a file LINK statement, keep the line as-is
                result.AppendLine(line);
            }

            return result.ToString();
        }

        /// <summary>
        ///     Creates a new interpreter instance.
        /// </summary>
        public static PowerScriptInterpreter CreateNew()
        {
            return new PowerScriptInterpreter();
        }
    }
}