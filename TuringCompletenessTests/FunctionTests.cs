using NUnit.Framework;
using ppotepa.tokenez.Interpreter;

namespace TuringCompletenessTests;

/// <summary>
/// NUnit tests for PowerScript Turing Completeness
/// Requirement #4: Function Calls & Recursion
/// </summary>
[TestFixture]
public class FunctionTests
{
    private PowerScriptInterpreter _interpreter = null!;

    [SetUp]
    public void Setup()
    {
        _interpreter = new PowerScriptInterpreter();
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Functions")]
    public void Test_SimpleFunctionDeclaration_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/30_SimpleFunction.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Functions")]
    public void Test_FunctionWithParameters_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/31_FunctionWithParams.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Functions")]
    public void Test_FunctionWithReturn_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/32_FunctionWithReturn.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Functions")]
    public void Test_RecursiveFunction_ShouldExecuteCorrectly()
    {
        var script = File.ReadAllText("TestScripts/33_RecursiveFunction.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }
}
