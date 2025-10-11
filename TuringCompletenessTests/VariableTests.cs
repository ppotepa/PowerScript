using NUnit.Framework;
using ppotepa.tokenez.Interpreter;

namespace TuringCompletenessTests;

/// <summary>
/// NUnit tests for PowerScript Turing Completeness
/// Requirement #2: Arbitrary Memory (Variables)
/// </summary>
[TestFixture]
public class VariableTests
{
    private PowerScriptInterpreter _interpreter = null!;

    [SetUp]
    public void Setup()
    {
        _interpreter = new PowerScriptInterpreter();
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Variables")]
    public void Test_FlexVariables_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/10_FlexVariables.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Variables")]
    public void Test_TypedVariables_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/11_TypedVariables.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Variables")]
    public void Test_VariableReassignment_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/12_VariableReassignment.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Variables")]
    public void Test_ScopedVariables_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/13_ScopedVariables.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }
}
