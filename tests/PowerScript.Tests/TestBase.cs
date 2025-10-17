using PowerScript.Interpreter;
using PowerScript.Compiler;
using PowerScript.Compiler.Interfaces;
using PowerScript.Runtime;
using PowerScript.Runtime.Interfaces;
using PowerScript.Parser.Processors.Base;
using PowerScript.Parser.Processors.Statements;
using PowerScript.Parser.Processors.ControlFlow;
using PowerScript.Parser.Processors.Expressions;
using PowerScript.Parser.Processors.Scoping;
using PowerScript.Core.DotNet;
using PowerScript.Interpreter.DotNet;
using PowerScript.Common.Logging;
using System.Text;
using NUnit.Framework;

namespace PowerScript.Tests;

/// <summary>
/// Base class for all PowerScript tests.
/// Provides script execution and expected output parsing functionality.
/// </summary>
public abstract class TestBase
{
    protected PowerScriptInterpreter Interpreter { get; private set; } = null!;

    [SetUp]
    public void Setup()
    {
        // Disable logging for tests
        LoggerService.UseNullLogger();

        // Initialize the token processor registry
        var registry = new TokenProcessorRegistry();
        var dotNetLinker = new DotNetLinker();
        var scopeBuilder = new ScopeBuilder(registry);

        // Register all processors - ORDER MATTERS!
        var parameterProcessor = new ParameterProcessor();
        registry.Register(new FunctionProcessor(parameterProcessor));  // MUST be first for FUNC keyword
        registry.Register(new StaticTypeVariableProcessor());
        registry.Register(new NetMemberAccessStatementProcessor());
        registry.Register(new LinkStatementProcessor());
        registry.Register(new FlexVariableProcessor());
        registry.Register(new VariableAssignmentProcessor());
        registry.Register(new CycleLoopProcessor(scopeBuilder));
        registry.Register(new IfStatementProcessor(scopeBuilder));
        registry.Register(new ReturnStatementProcessor());
        // PrintStatementProcessor removed - PRINT is now a library function
        registry.Register(new FunctionCallStatementProcessor());
        registry.Register(new FunctionCallProcessor());
        registry.Register(new ExecuteCommandProcessor());
        registry.Register(new NetMethodCallProcessor());
        registry.Register(new VariableDeclarationProcessor());

        // Create compiler and executor
        var compiler = new PowerScriptCompilerNew(registry, dotNetLinker, scopeBuilder);
        var executor = new PowerScriptExecutor();

        Interpreter = new PowerScriptInterpreter(compiler, executor);

        // Load standard library
        LoadStandardLibrary();
    }

    /// <summary>
    /// Loads the PowerScript standard library.
    /// </summary>
    private void LoadStandardLibrary()
    {
        string stdlibPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "..",
            "stdlib", "StandardLibrary.ps"
        );

        if (File.Exists(stdlibPath))
        {
            try
            {
                // Load stdlib silently (capture ALL output including errors)
                using var outputCapture = new StringWriter();
                using var errorCapture = new StringWriter();
                var originalOut = Console.Out;
                var originalError = Console.Error;

                try
                {
                    Console.SetOut(outputCapture);
                    Console.SetError(errorCapture);
                    Interpreter.ExecuteFile(stdlibPath);
                }
                finally
                {
                    Console.SetOut(originalOut);
                    Console.SetError(originalError);
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Warning: Failed to load standard library: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Executes a PowerScript code string and returns the output.
    /// </summary>
    protected string ExecuteCode(string code)
    {
        using var outputCapture = new StringWriter();
        var originalOut = Console.Out;

        try
        {
            Console.SetOut(outputCapture);
            Interpreter.ExecuteCode(code);
            return outputCapture.ToString().Trim();
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Executes a PowerScript file and returns the output.
    /// </summary>
    protected string ExecuteScriptFile(string scriptPath)
    {
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException($"Test script not found: {scriptPath}");
        }

        string code = File.ReadAllText(scriptPath);
        return ExecuteCode(code);
    }

    /// <summary>
    /// Parses expected output from a script file.
    /// Format: // EXPECTED: output (can have multiple lines)
    /// Or multi-line:
    /// // EXPECTED_START
    /// // line 1
    /// // line 2
    /// // EXPECTED_END
    /// </summary>
    protected string ParseExpectedOutput(string scriptPath)
    {
        string content = File.ReadAllText(scriptPath);

        // Check for multiple single-line expected outputs
        var singleLineMatches = System.Text.RegularExpressions.Regex.Matches(
            content,
            @"^//\s*EXPECTED:\s*(.+)$",
            System.Text.RegularExpressions.RegexOptions.Multiline
        );

        if (singleLineMatches.Count > 0)
        {
            var expectedLines = singleLineMatches
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(m => m.Groups[1].Value.Trim());

            return string.Join(Environment.NewLine, expectedLines);
        }

        // Check for multi-line expected output
        var multiLineMatch = System.Text.RegularExpressions.Regex.Match(
            content,
            @"^//\s*EXPECTED_START\s*$(.+?)^//\s*EXPECTED_END\s*$",
            System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.Singleline
        );

        if (multiLineMatch.Success)
        {
            var lines = multiLineMatch.Groups[1].Value
                .Split('\n')
                .Select(line => line.TrimStart('/').Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line));

            return string.Join(Environment.NewLine, lines).Trim();
        }

        throw new InvalidOperationException(
            $"No expected output found in {Path.GetFileName(scriptPath)}. " +
            "Add '// EXPECTED: <output>' or '// EXPECTED_START ... // EXPECTED_END' at the top of the file."
        );
    }

    /// <summary>
    /// Gets all test scripts from a directory.
    /// </summary>
    protected static IEnumerable<string> GetTestScripts(string relativePath)
    {
        string baseDirectory = TestContext.CurrentContext.TestDirectory;
        string fullPath = Path.Combine(baseDirectory, relativePath);

        if (!Directory.Exists(fullPath))
        {
            yield break;
        }

        foreach (string file in Directory.GetFiles(fullPath, "*.ps", SearchOption.AllDirectories))
        {
            yield return file;
        }
    }
}
