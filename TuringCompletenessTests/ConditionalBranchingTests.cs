using NUnit.Framework;
using ppotepa.tokenez.Interpreter;

namespace TuringCompletenessTests;

/// <summary>
/// NUnit tests for PowerScript Turing Completeness
/// Requirement #1: Conditional Branching (IF/ELSE/AND/OR)
/// </summary>
[TestFixture]
public class ConditionalBranchingTests
{
    private PowerScriptInterpreter _interpreter = null!;

    [SetUp]
    public void Setup()
    {
        _interpreter = new PowerScriptInterpreter();
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Conditionals")]
    public void Test_SimpleIF_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/01_SimpleIF.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Conditionals")]
    public void Test_IFWithELSE_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/02_IFWithELSE.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Conditionals")]
    public void Test_LogicalAND_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/03_LogicalAND.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Conditionals")]
    public void Test_LogicalOR_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/04_LogicalOR.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Conditionals")]
    public void Test_NestedConditionals_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/05_NestedConditionals.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Conditionals")]
    public void Test_ComplexConditions_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/06_ComplexConditions.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Conditionals")]
    public void Test_AllComparisonOperators_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/07_AllComparisonOperators.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }
}
