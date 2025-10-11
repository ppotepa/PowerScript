using NUnit.Framework;
using ppotepa.tokenez.Interpreter;

namespace TuringCompletenessTests;

/// <summary>
/// Integration tests combining all Turing completeness requirements
/// </summary>
[TestFixture]
public class IntegrationTests
{
    private PowerScriptInterpreter _interpreter = null!;

    [SetUp]
    public void Setup()
    {
        _interpreter = new PowerScriptInterpreter();
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Integration")]
    public void Test_StateMachine_AllFeaturesCombined()
    {
        var script = File.ReadAllText("TestScripts/40_StateMachine.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Integration")]
    public void Test_AlgorithmicComputation_AllFeaturesCombined()
    {
        var script = File.ReadAllText("TestScripts/41_Algorithm.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }

    [Test]
    [Category("TuringCompleteness")]
    [Category("Integration")]
    public void Test_ComplexProgram_AllFeaturesCombined()
    {
        var script = File.ReadAllText("TestScripts/42_ComplexProgram.ps");
        Assert.DoesNotThrow(() => _interpreter.ExecuteCode(script));
    }
}
