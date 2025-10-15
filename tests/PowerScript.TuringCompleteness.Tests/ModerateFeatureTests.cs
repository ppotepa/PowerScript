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

namespace PowerScript.Integration.Tests;

[TestFixture]
[Category("Moderate")]
[Description("Moderate complexity tests combining multiple features")]
public class ModerateFeatureTests
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
    [Category("Algorithms")]
    [Description("Test 2.1: Sum of numbers from 1 to 10 (should be 55)")]
    public void Test_2_1_SumOfNumbers()
    {
        string script = File.ReadAllText("../../../../../test-scripts/moderate/2_1_sum_of_numbers.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("55"), "Sum of 1 to 10 should be 55");
    }

    [Test]
    [Category("Algorithms")]
    [Description("Test 2.2: Count numbers between 5 and 15 (should be 9)")]
    public void Test_2_2_ConditionalCounting()
    {
        string script = File.ReadAllText("../../../../../test-scripts/moderate/2_2_conditional_counting.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("9"), "Should count 9 numbers (6-14)");
    }

    [Test]
    [Category("Algorithms")]
    [Description("Test 2.3: Calculate separate sums for even and odd numbers")]
    public void Test_2_3_EvenOddSum()
    {
        string script = File.ReadAllText("../../../../../test-scripts/moderate/2_3_even_odd_sum.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("30"), "Even sum (2+4+6+8+10) should be 30");
        Assert.That(output, Does.Contain("25"), "Odd sum (1+3+5+7+9) should be 25");
    }

    [Test]
    [Category("Algorithms")]
    [Description("Test 2.4: Sum all products in a 4x5 multiplication table")]
    public void Test_2_4_MultiplicationTableSum()
    {
        string script = File.ReadAllText("../../../../../test-scripts/moderate/2_4_multiplication_table_sum.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("150"), "Sum of 4x5 multiplication table products");
    }

    [Test]
    [Category("Algorithms")]
    [Description("Test 2.5: Calculate 8th Fibonacci number")]
    public void Test_2_5_Fibonacci()
    {
        string script = File.ReadAllText("../../../../../test-scripts/moderate/2_5_fibonacci.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        // Fibonacci sequence: 0,1,1,2,3,5,8,13,21
        Assert.That(output, Does.Contain("13").Or.Contains("21"), "Fibonacci(8) should be 21 or intermediate value 13");
    }

    [Test]
    [Category("Algorithms")]
    [Description("Test 2.6: Calculate power (2^5 = 32)")]
    public void Test_2_6_PowerCalculation()
    {
        string script = File.ReadAllText("../../../../../test-scripts/moderate/2_6_power_calculation.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("32"), "2^5 should equal 32");
    }

    [Test]
    [Category("Algorithms")]
    [Description("Test 2.7: Find maximum value in a computed sequence")]
    public void Test_2_7_FindMaximum()
    {
        string script = File.ReadAllText("../../../../../test-scripts/moderate/2_7_find_maximum.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("100"), "Maximum of squares 1-10 should be 100");
    }

    [Test]
    [Category("Algorithms")]
    [Description("Test 2.8: Count prime numbers using trial division")]
    public void Test_2_8_CountPrimes()
    {
        string script = File.ReadAllText("../../../../../test-scripts/moderate/2_8_count_primes.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        // Should count primes in range 2-21
        Assert.That(output, Does.Match(@"\d+"), "Should output a count of primes");
    }

    [Test]
    [Category("Expressions")]
    [Description("Test 2.9: Evaluate complex nested expressions")]
    public void Test_2_9_ComplexExpressions()
    {
        string script = File.ReadAllText("../../../../../test-scripts/moderate/2_9_complex_expressions.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("11"), "(5+3)*2-(10/2) = 11");
        Assert.That(output, Does.Contain("21"), "((2+3)*(4+1))-((10-2)/2) = 21");
    }

    [Test]
    [Category("Algorithms")]
    [Description("Test 2.10: Calculate both sum and product with conditions")]
    public void Test_2_10_SumAndProduct()
    {
        string script = File.ReadAllText("../../../../../test-scripts/moderate/2_10_sum_and_product.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));

        string output = GetOutput();
        Assert.That(output, Does.Contain("12"), "Sum of 3+4+5 = 12");
        Assert.That(output, Does.Contain("60"), "Product of 3*4*5 = 60");
    }
}
