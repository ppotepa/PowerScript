using NUnit.Framework;
using ppotepa.tokenez.Interpreter;

namespace TuringCompletenessTests;

/// <summary>
/// NUnit tests for PowerScript Turing Completeness
/// Requirement #3: Iteration (Loops)
/// </summary>
[TestFixture]
public class LoopTests
{
    private PowerScriptInterpreter _interpreter = null!;

    [SetUp]
    public void Setup()
    {
        _interpreter = new PowerScriptInterpreter();
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Loops")]
    public void Test_CycleLoop_SyntaxParsing()
    {
        var script = File.ReadAllText("TestScripts/20_CycleLoop.ps");
        // For now, just verify syntax parsing works
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Loops")]
    public void Test_CycleWithAS_SyntaxParsing()
    {
        var script = File.ReadAllText("TestScripts/21_CycleWithAS.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Loops")]
    public void Test_NestedCycles_SyntaxParsing()
    {
        var script = File.ReadAllText("TestScripts/22_NestedCycles.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }
}
