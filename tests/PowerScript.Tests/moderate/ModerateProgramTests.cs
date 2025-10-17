using NUnit.Framework;

namespace PowerScript.Tests.Moderate;

/// <summary>
/// Moderate complexity tests (10-30 lines, multiple concepts).
/// Examples: Factorial, Fibonacci, prime detection, array manipulation
/// </summary>
[TestFixture]
public class ModerateProgramTests : TestBase
{
    [Test]
    [TestCaseSource(nameof(GetModerateScripts))]
    public void ModerateProgram_ProducesCorrectOutput(string scriptPath)
    {
        string testName = Path.GetFileNameWithoutExtension(scriptPath);
        string expectedOutput = ParseExpectedOutput(scriptPath);

        TestContext.WriteLine($"Test: {testName}");
        TestContext.WriteLine($"Expected: {expectedOutput}");

        string actualOutput = ExecuteScriptFile(scriptPath);

        TestContext.WriteLine($"Actual: {actualOutput}");

        Assert.That(actualOutput, Is.EqualTo(expectedOutput),
            $"Moderate program '{testName}' produced incorrect output");
    }

    private static IEnumerable<TestCaseData> GetModerateScripts()
    {
        foreach (string scriptPath in GetTestScripts("moderate/scripts"))
        {
            string testName = Path.GetFileNameWithoutExtension(scriptPath);
            yield return new TestCaseData(scriptPath).SetName(testName);
        }
    }
}
