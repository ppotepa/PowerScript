using NUnit.Framework;

namespace PowerScript.Tests.Complex;

/// <summary>
/// Complex program tests (30+ lines, algorithms, data structures).
/// Examples: Sorting algorithms, binary search, recursion, state machines
/// </summary>
[TestFixture]
public class ComplexProgramTests : TestBase
{
    [Test]
    [TestCaseSource(nameof(GetComplexScripts))]
    public void ComplexProgram_ProducesCorrectOutput(string scriptPath)
    {
        string testName = Path.GetFileNameWithoutExtension(scriptPath);
        string expectedOutput = ParseExpectedOutput(scriptPath);

        TestContext.WriteLine($"Test: {testName}");
        TestContext.WriteLine($"Expected: {expectedOutput}");

        string actualOutput = ExecuteScriptFile(scriptPath);

        TestContext.WriteLine($"Actual: {actualOutput}");

        Assert.That(actualOutput, Is.EqualTo(expectedOutput),
            $"Complex program '{testName}' produced incorrect output");
    }

    private static IEnumerable<TestCaseData> GetComplexScripts()
    {
        foreach (string scriptPath in GetTestScripts("complex/scripts"))
        {
            string testName = Path.GetFileNameWithoutExtension(scriptPath);
            yield return new TestCaseData(scriptPath).SetName(testName);
        }
    }
}
