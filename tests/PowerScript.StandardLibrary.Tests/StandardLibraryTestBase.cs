using NUnit.Framework;
using PowerScript.Compiler;
using PowerScript.Interpreter;
using PowerScript.Interpreter.DotNet;
using PowerScript.Parser.Processors.Base;
using PowerScript.Parser.Processors.ControlFlow;
using PowerScript.Parser.Processors.Expressions;
using PowerScript.Parser.Processors.Scoping;
using PowerScript.Parser.Processors.Statements;
using PowerScript.Runtime;
using PowerScript.Common.Logging;
using System.IO;
using System.Text;

namespace PowerScript.StandardLibrary.Tests;

[TestFixture]
public abstract class StandardLibraryTestBase
{
    protected PowerScriptInterpreter Interpreter { get; private set; } = null!;
    protected StringBuilder OutputCapture { get; private set; } = null!;
    protected StringWriter OutputWriter { get; private set; } = null!;

    [SetUp]
    public void Setup()
    {
        // Disable debug logging for clean test output
        LoggerService.UseNullLogger();

        var registry = new TokenProcessorRegistry();
        var dotNetLinker = new DotNetLinker();
        var scopeBuilder = new ScopeBuilder(registry);

        RegisterProcessors(registry, scopeBuilder);

        var compiler = new PowerScriptCompilerNew(registry, dotNetLinker, scopeBuilder);
        var executor = new PowerScriptExecutor();
        Interpreter = new PowerScriptInterpreter(compiler, executor);

        OutputCapture = new StringBuilder();
        OutputWriter = new StringWriter(OutputCapture);
        Console.SetOut(OutputWriter);
    }

    [TearDown]
    public void TearDown()
    {
        OutputWriter.Dispose();
        var standardOutput = new StreamWriter(Console.OpenStandardOutput());
        standardOutput.AutoFlush = true;
        Console.SetOut(standardOutput);
    }

    protected string ExecuteCode(string code)
    {
        OutputCapture.Clear();
        Interpreter.ExecuteCode(code);
        return OutputCapture.ToString().Trim();
    }

    private void RegisterProcessors(TokenProcessorRegistry registry, ScopeBuilder scopeBuilder)
    {
        var parameterProcessor = new ParameterProcessor();

        registry.Register(new StaticTypeVariableProcessor()); // INT, STRING, NUMBER
        registry.Register(new FunctionProcessor(parameterProcessor));
        registry.Register(new NetMemberAccessStatementProcessor()); // Console -> WriteLine(42)
        registry.Register(new LinkStatementProcessor()); // LINK System or LINK "file.ps"
        registry.Register(new FlexVariableProcessor());
        registry.Register(new VariableAssignmentProcessor()); // identifier = value
        registry.Register(new CycleLoopProcessor(scopeBuilder));
        registry.Register(new IfStatementProcessor(scopeBuilder));
        registry.Register(new ReturnStatementProcessor());
        // NOTE: PrintStatementProcessor removed - PRINT is now a function in stdlib/IO.ps
        registry.Register(new FunctionCallStatementProcessor()); // Single-param: PRINT x, Multi-param: FUNC(a,b)
        registry.Register(new FunctionCallProcessor());
        registry.Register(new ExecuteCommandProcessor());
        registry.Register(new NetMethodCallProcessor());
        registry.Register(new VariableDeclarationProcessor()); // VAR
    }
}
