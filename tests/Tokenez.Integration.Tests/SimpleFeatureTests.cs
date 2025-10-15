using NUnit.Framework;
using Tokenez.Compiler;
using Tokenez.Interpreter;
using Tokenez.Interpreter.DotNet;
using Tokenez.Parser.Processors.Base;
using Tokenez.Parser.Processors.Scoping;
using Tokenez.Parser.Processors.Statements;
using Tokenez.Parser.Processors.Expressions;
using Tokenez.Parser.Processors.ControlFlow;
using Tokenez.Runtime;

namespace Tokenez.Integration.Tests;

[TestFixture]
[Category("Simple")]
[Description("Simple feature tests covering basic language constructs")]
public class SimpleFeatureTests
{
    [SetUp]
    public void Setup()
    {
        // Initialize the new separated domains architecture
        var registry = new TokenProcessorRegistry();
        var dotNetLinker = new DotNetLinker();
        var scopeBuilder = new ScopeBuilder(registry);

        // Register all the processors (like CLI does)
        RegisterProcessors(registry, scopeBuilder);

        var compiler = new PowerScriptCompilerNew(registry, dotNetLinker, scopeBuilder);
        var executor = new PowerScriptExecutor();
        _interpreter = new PowerScriptInterpreter(compiler, executor);

        // Link the standard library
        string stdLibPath = Path.Combine("..", "..", "scripts", "stdlib", "StdLib.ps");
        if (File.Exists(stdLibPath))
        {
            // TODO: Update this to use the new executor's LinkLibrary method
            // _interpreter.LinkLibrary(stdLibPath);
        }

        _output = new StringWriter();
        Console.SetOut(_output);
    }

    private void RegisterProcessors(TokenProcessorRegistry registry, ScopeBuilder scopeBuilder)
    {
        // Create parameter processor (helper, not a token processor)
        var parameterProcessor = new ParameterProcessor();

        // Register all token processors (same as CLI)
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
        // NOTE: ScopeProcessor should NOT be registered here as it creates circular reference
        // The ScopeBuilder already handles scope processing internally
        // registry.Register(new ScopeProcessor(registry, scopeBuilder));
    }

    [TearDown]
    public void TearDown()
    {
        _output?.Dispose();
        Console.SetOut(Console.Out);
    }

    private PowerScriptInterpreter _interpreter = null!; // initialized in SetUp
    private StringWriter _output = null!; // initialized in SetUp

    private string GetOutput()
    {
        return _output.ToString();
    }

    [Test]
    [Category("Variables")]
    [Description("Test 1.1: Basic variable declaration and assignment")]
    public void Test_1_1_Variables()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_1_variables.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("5"), "Should print x = 5");
        Assert.That(output, Does.Contain("10"), "Should print y = 10");
        Assert.That(output, Does.Contain("15"), "Should print z = 15");
    }

    [Test]
    [Category("Arithmetic")]
    [Description("Test 1.2: Arithmetic operations (+, -, *, /)")]
    public void Test_1_2_Arithmetic()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_2_arithmetic.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("13"), "Should print sum = 13");
        Assert.That(output, Does.Contain("7"), "Should print diff = 7");
        Assert.That(output, Does.Contain("30"), "Should print product = 30");
        Assert.That(output, Does.Contain("3"), "Should print quotient = 3");
    }

    [Test]
    [Category("Conditionals")]
    [Description("Test 1.3: Simple IF/ELSE conditional statements")]
    public void Test_1_3_ConditionalSimple()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_3_conditional_simple.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("x is greater than 5"), "Should execute true branch for x");
        Assert.That(output, Does.Contain("y is not greater than 5"), "Should execute else branch for y");
    }

    [Test]
    [Category("Loops")]
    [Description("Test 1.4: Simple CYCLE loop with accumulation")]
    public void Test_1_4_LoopSimple()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_4_loop_simple.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("5"), "Should print sum = 5 after 5 iterations");
    }

    [Test]
    [Category("Loops")]
    [Description("Test 1.5: CYCLE loop with counter variable")]
    public void Test_1_5_LoopCounter()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_5_loop_counter.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("1"), "Should print 1");
        Assert.That(output, Does.Contain("2"), "Should print 2");
        Assert.That(output, Does.Contain("3"), "Should print 3");
    }

    [Test]
    [Category("Algorithms")]
    [Description("Test 1.6: Factorial calculation (5! = 120)")]
    public void Test_1_6_Factorial()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_6_factorial.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("120"), "Should calculate 5! = 120");
    }

    [Test]
    [Category("Expressions")]
    [Description("Test 1.7: Expression evaluation with parentheses")]
    public void Test_1_7_Parentheses()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_7_parentheses.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("20"), "Should calculate (2 + 3) * 4 = 20");
        Assert.That(output, Does.Contain("13"), "Should calculate ((1 + 2) * 3) + 4 = 13");
    }

    [Test]
    [Category("BooleanLogic")]
    [Description("Test 1.8: AND boolean logic")]
    public void Test_1_8_BooleanAnd()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_8_boolean_and.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("Both conditions true"), "Should evaluate AND correctly");
    }

    [Test]
    [Category("BooleanLogic")]
    [Description("Test 1.9: OR boolean logic")]
    public void Test_1_9_BooleanOr()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_9_boolean_or.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("At least one condition true"), "Should evaluate OR correctly");
    }

    [Test]
    [Category("Conditionals")]
    [Description("Test 1.10: Nested IF/ELSE statements")]
    public void Test_1_10_NestedConditionals()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_10_nested_conditionals.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("Medium").Or.Contain("MEDIUM"), "Should execute nested conditional correctly");
    }

    [Test]
    [Category("Loops")]
    [Description("Test 1.11: Nested CYCLE loops")]
    public void Test_1_11_NestedLoops()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_11_nested_loops.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        // 3x3 grid should produce products: 1,2,3,2,4,6,3,6,9
        Assert.That(output, Does.Contain("1"), "Should calculate 1*1 = 1");
        Assert.That(output, Does.Contain("4"), "Should calculate 2*2 = 4");
        Assert.That(output, Does.Contain("9"), "Should calculate 3*3 = 9");
    }

    [Test]
    [Category("Integration")]
    [Description("Test 1.12: CYCLE loop combined with IF conditional")]
    public void Test_1_12_LoopWithConditional()
    {
        string script = File.ReadAllText("../../../../../test-scripts/simple/1_12_loop_with_conditional.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("5"), "Should count 5 numbers greater than 5");
    }
}
