using NUnit.Framework;

namespace PowerScript.Tests.Turing;

/// <summary>
/// Turing completeness tests - demonstrates computational universality.
/// Validates: recursion theory, Church-Turing thesis, universal computation
/// </summary>
[TestFixture]
public class TuringCompletenessTests : TestBase
{
    [Test]
    [TestCaseSource(nameof(GetTuringScripts))]
    public void TuringTest_DemonstratesComputationalCompleteness(string scriptPath)
    {
        string testName = Path.GetFileNameWithoutExtension(scriptPath);
        string expectedOutput = ParseExpectedOutput(scriptPath);

        TestContext.WriteLine($"Test: {testName}");
        TestContext.WriteLine($"Expected: {expectedOutput}");

        string actualOutput = ExecuteScriptFile(scriptPath);

        TestContext.WriteLine($"Actual: {actualOutput}");

        Assert.That(actualOutput, Is.EqualTo(expectedOutput),
            $"Turing completeness test '{testName}' produced incorrect output");
    }

    private static IEnumerable<TestCaseData> GetTuringScripts()
    {
        foreach (string scriptPath in GetTestScripts("turing/scripts"))
        {
            string testName = Path.GetFileNameWithoutExtension(scriptPath);
            yield return new TestCaseData(scriptPath).SetName(testName);
        }
    }
}
