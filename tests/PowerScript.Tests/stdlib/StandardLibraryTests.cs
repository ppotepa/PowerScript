using NUnit.Framework;

namespace PowerScript.Tests.Stdlib;

/// <summary>
/// Standard library function tests.
/// Validates: Math, String, Array, Validation, I/O, Formatting functions
/// </summary>
[TestFixture]
public class StandardLibraryTests : TestBase
{
    [Test]
    [TestCaseSource(nameof(GetStdlibScripts))]
    public void StdlibFunction_ProducesCorrectOutput(string scriptPath)
    {
        string testName = Path.GetFileNameWithoutExtension(scriptPath);
        string expectedOutput = ParseExpectedOutput(scriptPath);

        TestContext.WriteLine($"Test: {testName}");
        TestContext.WriteLine($"Expected: {expectedOutput}");

        string actualOutput = ExecuteScriptFile(scriptPath);

        TestContext.WriteLine($"Actual: {actualOutput}");

        Assert.That(actualOutput, Is.EqualTo(expectedOutput),
            $"Standard library test '{testName}' produced incorrect output");
    }

    private static IEnumerable<TestCaseData> GetStdlibScripts()
    {
        foreach (string scriptPath in GetTestScripts("stdlib/scripts"))
        {
            string testName = Path.GetFileNameWithoutExtension(scriptPath);
            yield return new TestCaseData(scriptPath).SetName(testName);
        }
    }
}
