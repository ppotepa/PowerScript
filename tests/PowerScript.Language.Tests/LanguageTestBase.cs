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

namespace PowerScript.Language.Tests;

/// <summary>
/// Base class for all language feature tests
/// </summary>
public abstract class LanguageTestBase
{
    protected PowerScriptInterpreter Interpreter = null!;
    protected StringWriter Output = null!;

    [SetUp]
    public void BaseSetup()
    {
        // Initialize the compiler and executor
        var registry = new TokenProcessorRegistry();
        var dotNetLinker = new DotNetLinker();
        var scopeBuilder = new ScopeBuilder(registry);

        // Register all processors
        RegisterProcessors(registry, scopeBuilder);

        var compiler = new PowerScriptCompilerNew(registry, dotNetLinker, scopeBuilder);
        var executor = new PowerScriptExecutor();
        Interpreter = new PowerScriptInterpreter(compiler, executor);

        // Capture console output
        Output = new StringWriter();
        Console.SetOut(Output);
    }

    [TearDown]
    public void BaseTearDown()
    {
        Output?.Dispose();
        Console.SetOut(Console.Out);
    }

    private void RegisterProcessors(TokenProcessorRegistry registry, ScopeBuilder scopeBuilder)
    {
        var parameterProcessor = new ParameterProcessor();

        // Order matters: most specific processors first
        registry.Register(new StaticTypeVariableProcessor());
        registry.Register(new FunctionProcessor(parameterProcessor));
        registry.Register(new FunctionCallProcessor());
        registry.Register(new FlexVariableProcessor());
        registry.Register(new CycleLoopProcessor(scopeBuilder));
        registry.Register(new IfStatementProcessor(scopeBuilder));
        registry.Register(new ReturnStatementProcessor());
        registry.Register(new PrintStatementProcessor());
        registry.Register(new ExecuteCommandProcessor());
        registry.Register(new NetMethodCallProcessor());
        registry.Register(new VariableDeclarationProcessor());
    }

    protected void ExecuteScript(string script)
    {
        Interpreter.ExecuteCode(script);
    }

    protected string GetOutput()
    {
        return Output.ToString();
    }
}
