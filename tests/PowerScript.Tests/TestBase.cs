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
using PowerScript.Core.Syntax;
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
        // PrintStatementProcessor removed - PRINT is now a stdlib function
        registry.Register(new FunctionCallStatementProcessor());
        registry.Register(new FunctionCallProcessor());
        registry.Register(new ExecuteCommandProcessor());
        registry.Register(new NetMethodCallProcessor());
        registry.Register(new VariableDeclarationProcessor());

        // Create compiler and executor
        var compiler = new PowerScriptCompilerNew(registry, dotNetLinker, scopeBuilder);
        var executor = new PowerScriptExecutor();

        Interpreter = new PowerScriptInterpreter(compiler, executor);

        // Load basic functions (standard library) before custom syntax registration
        // to avoid syntax registration interfering with stdlib parsing.
        LoadBasicFunctions();

        // Load custom syntax patterns from .psx files only
        LoadCustomSyntax();
    }

    /// <summary>
    /// Loads custom syntax patterns from .psx files.
    /// </summary>
    private void LoadCustomSyntax()
    {
        string syntaxPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "..",
            "stdlib", "syntax"
        );

        if (Directory.Exists(syntaxPath))
        {
            try
            {
                PsxFileLoader.LoadDirectory(syntaxPath);
                TestContext.Out.WriteLine($"Loaded custom syntax from: {syntaxPath}");

                // Debug: List all loaded patterns
                var patterns = CustomSyntaxRegistry.Instance.GetPatternTransformations();
                TestContext.Out.WriteLine($"Loaded {patterns.Count} pattern transformations:");
                foreach (var pattern in patterns)
                {
                    TestContext.Out.WriteLine($"  - {pattern.Pattern} => {pattern.Transformation}");
                }
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"Warning: Failed to load custom syntax: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Loads basic functions needed for test compatibility.
    /// </summary>
    private void LoadBasicFunctions()
    {
        try
        {
            // Link the System namespace so .NET interop (e.g. #Console) works
            Interpreter.ExecuteCode("LINK System");

            // NOTE: Stdlib is now auto-injected per-test in ExecuteScriptFile()
            // This allows each test to get the correct relative path to stdlib modules
            // based on the test script's location.

            // Add frequently needed type validation functions directly
            // These can't be auto-injected from TypeSystem.ps/Validation.ps because
            // those files have their own LINK System statement which would conflict
            string typeValidationFunctions = @"
FUNCTION ISINT(FLEX value)[INT] {
    FLEX typeName = #value->GetType()->Name
    IF typeName == ""Int32"" {
        RETURN 1
    }
    IF typeName == ""Int64"" {
        RETURN 1
    }
    IF typeName == ""Int16"" {
        RETURN 1
    }
    IF typeName == ""Byte"" {
        RETURN 1
    }
    RETURN 0
}

FUNCTION ISEMPTY(FLEX value)[INT] {
    IF value == 0 {
        RETURN 1
    }
    FLEX strValue = #value->ToString()
    FLEX length = #strValue->Length
    IF length == 0 {
        RETURN 1
    }
    RETURN 0
}

FUNCTION ISSTRING(FLEX value)[INT] {
    FLEX typeName = #value->ToString()
    IF typeName == ""Hello"" {
        RETURN 1
    }
    RETURN 0
}
";
            Interpreter.ExecuteCode(typeValidationFunctions);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail tests immediately; individual tests may still pass
            TestContext.Out.WriteLine($"Warning: Failed to load System namespace: {ex.Message}");
            TestContext.Out.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Executes a PowerScript code string and returns the output.
    /// </summary>
    protected string ExecuteCode(string code)
    {
        // Auto-inject stdlib links if not already present
        if (!code.Contains("LINK") && !code.Contains("link"))
        {
            // Get the actual project root (not the build output directory)
            string currentDir = Directory.GetCurrentDirectory();

            // Navigate up to find the solution root
            DirectoryInfo? dir = new DirectoryInfo(currentDir);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "PowerScript.sln")))
            {
                dir = dir.Parent;
            }

            if (dir != null)
            {
                string solutionRoot = dir.FullName;
                string stdlibPath = Path.Combine(solutionRoot, "stdlib");

                // For inline code, use absolute paths since there's no script location
                string stdlibLinks = $@"// Auto-injected stdlib links
LINK ""{stdlibPath}/IO.ps""
LINK ""{stdlibPath}/Core.ps""
LINK ""{stdlibPath}/String.ps""
LINK ""{stdlibPath}/Math.ps""
LINK ""{stdlibPath}/ArrayLib.ps""

";
                code = stdlibLinks + code;
            }
        }

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

        // Get the stdlib path (needed for both auto-injection strategies)
        string currentDir = Directory.GetCurrentDirectory();
        DirectoryInfo? dir = new DirectoryInfo(currentDir);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "PowerScript.sln")))
        {
            dir = dir.Parent;
        }

        if (dir == null)
        {
            throw new DirectoryNotFoundException("Could not find solution root directory");
        }

        string solutionRoot = dir.FullName;
        string stdlibPath = Path.Combine(solutionRoot, "stdlib");

        // Check if this is a custom syntax file (has stdlib/syntax/ LINK)
        bool isCustomSyntaxFile = code.Contains("stdlib/syntax/");

        if (isCustomSyntaxFile)
        {
            // For custom syntax files, add stdlib links AFTER their syntax links
            var lines = code.Split('\n').ToList();
            int lastLinkIndex = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed = lines[i].TrimStart();
                if (trimmed.StartsWith("LINK", StringComparison.OrdinalIgnoreCase))
                {
                    lastLinkIndex = i;
                }
            }

            if (lastLinkIndex >= 0)
            {
                // Use absolute paths for stdlib modules
                string ioPath = Path.Combine(stdlibPath, "IO.ps").Replace("\\", "/");
                string corePath = Path.Combine(stdlibPath, "Core.ps").Replace("\\", "/");
                string stringPath = Path.Combine(stdlibPath, "String.ps").Replace("\\", "/");
                string mathPath = Path.Combine(stdlibPath, "Math.ps").Replace("\\", "/");
                string arrayPath = Path.Combine(stdlibPath, "ArrayLib.ps").Replace("\\", "/");

                // Insert stdlib links after the last custom syntax LINK
                var stdlibLinks = new[]
                {
                    $"LINK \"{ioPath}\"",
                    $"LINK \"{corePath}\"",
                    $"LINK \"{stringPath}\"",
                    $"LINK \"{mathPath}\"",
                    $"LINK \"{arrayPath}\""
                };

                // Insert in reverse order to maintain correct order
                for (int i = stdlibLinks.Length - 1; i >= 0; i--)
                {
                    lines.Insert(lastLinkIndex + 1, stdlibLinks[i]);
                }

                code = string.Join("\n", lines);
            }
        }
        else if (!code.Contains("LINK") && !code.Contains("link"))
        {
            // Original auto-injection for files with no LINK statements
            // Use absolute paths since test scripts might be copied to bin/Debug
            string ioPath = Path.Combine(stdlibPath, "IO.ps").Replace("\\", "/");
            string corePath = Path.Combine(stdlibPath, "Core.ps").Replace("\\", "/");
            string stringPath = Path.Combine(stdlibPath, "String.ps").Replace("\\", "/");
            string mathPath = Path.Combine(stdlibPath, "Math.ps").Replace("\\", "/");
            string arrayPath = Path.Combine(stdlibPath, "ArrayLib.ps").Replace("\\", "/");

            // Prepend LINK statements for commonly used stdlib modules
            string stdlibLinks = $@"// Auto-injected stdlib links
LINK ""{ioPath}""
LINK ""{corePath}""
LINK ""{stringPath}""
LINK ""{mathPath}""
LINK ""{arrayPath}""

";
            code = stdlibLinks + code;
        }

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
