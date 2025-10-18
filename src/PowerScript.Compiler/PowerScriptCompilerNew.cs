using System.Diagnostics;
using PowerScript.Common.Logging;
using PowerScript.Compiler.Interfaces;
using PowerScript.Compiler.Models;
using PowerScript.Core.AST;
using PowerScript.Core.DotNet;
using PowerScript.Parser.Lexer;
using PowerScript.Parser.Processors.Base;
using PowerScript.Parser.Prompt;

namespace PowerScript.Compiler;

/// <summary>
/// PowerScript compiler that transforms source code into executable artifacts.
/// This class is responsible ONLY for compilation - not execution.
/// </summary>
public class PowerScriptCompilerNew : IPowerScriptCompilerNew
{
    private readonly ITokenProcessorRegistry _registry;
    private readonly IDotNetLinker _dotNetLinker;
    private readonly IScopeBuilder _scopeBuilder;

    public PowerScriptCompilerNew(
        ITokenProcessorRegistry registry,
        IDotNetLinker dotNetLinker,
        IScopeBuilder scopeBuilder)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _dotNetLinker = dotNetLinker ?? throw new ArgumentNullException(nameof(dotNetLinker));
        _scopeBuilder = scopeBuilder ?? throw new ArgumentNullException(nameof(scopeBuilder));
    }

    /// <summary>
    /// Compiles PowerScript source code into executable artifacts.
    /// </summary>
    public CompilationResult Compile(string sourceCode, string? sourceFile = null)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
        {
            return CompilationResult.Failed(new[] { "Source code cannot be null or empty" });
        }

        var startTime = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            LoggerService.Logger.Debug("[COMPILER] Starting compilation");

            // Preprocess to inline LINK statements
            sourceCode = PreprocessLinkStatements(sourceCode, sourceFile);

            // Create prompt and build token tree
            var prompt = new UserPrompt(sourceCode);
            var tokenTree = new TokenTree(_registry, _dotNetLinker, _scopeBuilder).Create(prompt);

            if (tokenTree.RootScope == null)
            {
                return CompilationResult.Failed(new[] { "Failed to create root scope during compilation" });
            }

            // Extract function declarations
            var functions = ExtractFunctions(tokenTree.RootScope);

            // Create compilation statistics
            var statistics = CreateStatistics(tokenTree.RootScope, sourceCode);

            // Create metadata
            stopwatch.Stop();
            var metadata = new CompilationMetadata(
                startTime,
                stopwatch.Elapsed,
                sourceCode,
                sourceFile,
                statistics);

            LoggerService.Logger.Debug($"[COMPILER] Compilation completed successfully in {stopwatch.ElapsedMilliseconds}ms");

            return new CompilationResult(
                tokenTree,
                tokenTree.RootScope,
                functions,
                metadata);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LoggerService.Logger.Error($"[COMPILER] Compilation failed: {ex.Message}");

            var metadata = new CompilationMetadata(
                startTime,
                stopwatch.Elapsed,
                sourceCode,
                sourceFile);

            return CompilationResult.Failed(new[] { ex.Message }, metadata);
        }
    }

    /// <summary>
    /// Preprocesses source code to inline LINK statements.
    /// </summary>
    private string PreprocessLinkStatements(string sourceCode, string? sourceFile)
    {
        // Keep preprocessing until no more LINK directives are found (recursive linking)
        string current = sourceCode;
        string previous;
        int maxIterations = 10; // Prevent infinite loops
        int iteration = 0;

        do
        {
            previous = current;
            current = ProcessLinkStatementsOnce(current, sourceFile);
            iteration++;
            LoggerService.Logger.Debug($"[COMPILER] Preprocessing iteration {iteration}: {previous.Length} -> {current.Length} chars");
        }
        while (current != previous && iteration < maxIterations);

        if (iteration >= maxIterations)
        {
            LoggerService.Logger.Warning($"[COMPILER] Maximum LINK preprocessing iterations ({maxIterations}) reached - possible circular dependencies");
        }

        LoggerService.Logger.Debug($"[COMPILER] Final preprocessed output:\n{current}");

        return current;
    }

    /// <summary>
    /// Single pass of LINK statement preprocessing.
    /// </summary>
    private string ProcessLinkStatementsOnce(string sourceCode, string? sourceFile)
    {
        var lines = sourceCode.Split('\n');
        var result = new List<string>();
        // Collect .psx / SYNTAX links and append them at the end of this pass
        var deferredSyntax = new List<string>();
        var linkPattern = new System.Text.RegularExpressions.Regex(@"^\s*LINK(?:\s+(SYNTAX))?\s+""([^""]+)""\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (var line in lines)
        {
            var match = linkPattern.Match(line);
            if (match.Success)
            {
                var isSyntaxDirective = !string.IsNullOrEmpty(match.Groups[1].Value);
                var linkedFile = match.Groups[2].Value;

                // Try multiple resolution strategies
                string? resolvedPath = null;

                // Strategy 1: Relative to current working directory (workspace root)
                var cwd = Directory.GetCurrentDirectory();
                var cwdPath = Path.Combine(cwd, linkedFile);
                if (File.Exists(cwdPath))
                {
                    resolvedPath = cwdPath;
                }

                // Strategy 2: Relative to source file directory
                if (resolvedPath == null && sourceFile != null)
                {
                    var basePath = Path.GetDirectoryName(sourceFile);
                    var relPath = Path.Combine(basePath ?? "", linkedFile);
                    if (File.Exists(relPath))
                    {
                        resolvedPath = relPath;
                    }
                }

                // Strategy 3: Absolute path
                if (resolvedPath == null && Path.IsPathRooted(linkedFile) && File.Exists(linkedFile))
                {
                    resolvedPath = linkedFile;
                }

                if (resolvedPath != null)
                {
                    // If this is a .psx file, load it via PsxFileLoader instead of inlining
                    var isPsx = string.Equals(Path.GetExtension(resolvedPath), ".psx", StringComparison.OrdinalIgnoreCase);
                    if (isSyntaxDirective || isPsx)
                    {
                        LoggerService.Logger.Debug($"[COMPILER] Loading .psx syntax file: {resolvedPath}");
                        // Load the .psx file to register its patterns
                        try
                        {
                            PsxFileLoader.LoadFile(resolvedPath);
                            LoggerService.Logger.Info($"[COMPILER] Loaded syntax patterns from: {resolvedPath}");
                        }
                        catch (Exception ex)
                        {
                            LoggerService.Logger.Warning($"[COMPILER] Failed to load .psx file {resolvedPath}: {ex.Message}");
                        }
                        // Don't inline .psx content - just add a comment marker
                        result.Add($"// SYNTAX LOADED: {linkedFile}");
                    }
                    else
                    {
                        LoggerService.Logger.Debug($"[COMPILER] Inlining linked file: {resolvedPath}");
                        var linkedContent = File.ReadAllText(resolvedPath);

                        // Recursively process LINK statements in the linked file,
                        // using the linked file's path as the new source file for relative resolution
                        var processedLinkedContent = ProcessLinkStatementsOnce(linkedContent, resolvedPath);

                        result.Add($"// LINK {linkedFile} - START");
                        result.Add(processedLinkedContent);
                        result.Add($"// LINK {linkedFile} - END");
                    }
                }
                else
                {
                    LoggerService.Logger.Warning($"[COMPILER] Linked file not found, keeping LINK statement: {linkedFile}");
                    result.Add(line); // Keep original LINK statement if file not found
                }
            }
            else
            {
                result.Add(line);
            }
        }
        // Note: deferredSyntax is no longer used since .psx files are loaded directly
        var preprocessed = string.Join('\n', result);
        LoggerService.Logger.Debug($"[COMPILER] Preprocessed {lines.Length} lines -> {result.Count} segments, {preprocessed.Split('\n').Length} final lines");
        return preprocessed;
    }

    /// <summary>
    /// Compiles PowerScript from a file.
    /// </summary>
    public CompilationResult CompileFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return CompilationResult.Failed(new[] { $"File not found: {filePath}" });
            }

            var sourceCode = File.ReadAllText(filePath);
            return Compile(sourceCode, filePath);
        }
        catch (Exception ex)
        {
            LoggerService.Logger.Error($"[COMPILER] Failed to read file {filePath}: {ex.Message}");
            return CompilationResult.Failed(new[] { $"Failed to read file: {ex.Message}" });
        }
    }

    private Dictionary<string, FunctionDeclaration> ExtractFunctions(Scope scope)
    {
        var functions = new Dictionary<string, FunctionDeclaration>();

        if (scope.Decarations != null)
        {
            foreach (var declaration in scope.Decarations.Values)
            {
                if (declaration is FunctionDeclaration funcDecl &&
                    !string.IsNullOrEmpty(funcDecl.Identifier?.RawToken?.Text))
                {
                    functions[funcDecl.Identifier.RawToken.Text] = funcDecl;
                }
            }
        }

        // Recursively extract from nested scopes
        if (scope.Statements != null)
        {
            foreach (var statement in scope.Statements)
            {
                ExtractFunctionsFromStatement(statement, functions);
            }
        }

        return functions;
    }

    private void ExtractFunctionsFromStatement(object statement, Dictionary<string, FunctionDeclaration> functions)
    {
        // Add logic to extract functions from different statement types if needed
        // For now, most functions should be at scope level
    }

    private CompilationStatistics CreateStatistics(Scope rootScope, string sourceCode)
    {
        var statementCount = CountStatements(rootScope);
        var functionCount = CountFunctions(rootScope);
        var lineCount = sourceCode.Split('\n').Length;

        return new CompilationStatistics
        {
            StatementCount = statementCount,
            FunctionCount = functionCount,
            VariableCount = 0, // TODO: Count variables if needed
            LineCount = lineCount
        };
    }

    private int CountStatements(Scope scope)
    {
        var count = 0;

        if (scope.Statements != null)
        {
            count += scope.Statements.Count;
        }

        // Add recursive counting for nested scopes if needed

        return count;
    }

    private int CountFunctions(Scope scope)
    {
        var count = 0;

        if (scope.Decarations != null)
        {
            count += scope.Decarations.Values.OfType<FunctionDeclaration>().Count();
        }

        return count;
    }
}