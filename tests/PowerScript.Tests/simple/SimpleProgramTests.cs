using NUnit.Framework;

namespace PowerScript.Tests.Simple;

/// <summary>
/// Simple program tests (1-10 lines, basic concepts).
/// Examples: Hello World, basic arithmetic, simple loops, variable assignment
/// </summary>
[TestFixture]
public class SimpleProgramTests : TestBase
{
    [Test]
    [TestCaseSource(nameof(GetSimpleScripts))]
    public void SimpleProgram_ProducesCorrectOutput(string scriptPath)
    {
        string testName = Path.GetFileNameWithoutExtension(scriptPath);
        string expectedOutput = ParseExpectedOutput(scriptPath);

        TestContext.WriteLine($"Test: {testName}");
        TestContext.WriteLine($"Expected: {expectedOutput}");

        string actualOutput = ExecuteScriptFile(scriptPath);

        TestContext.WriteLine($"Actual: {actualOutput}");

        Assert.That(actualOutput, Is.EqualTo(expectedOutput),
            $"Simple program '{testName}' produced incorrect output");
    }

    private static IEnumerable<TestCaseData> GetSimpleScripts()
    {
        foreach (string scriptPath in GetTestScripts("simple/scripts"))
        {
            string testName = Path.GetFileNameWithoutExtension(scriptPath);
            yield return new TestCaseData(scriptPath).SetName(testName);
        }
    }
}
