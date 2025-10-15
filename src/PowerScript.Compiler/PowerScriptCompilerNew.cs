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