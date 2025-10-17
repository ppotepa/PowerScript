using NUnit.Framework;

namespace PowerScript.Tests.Features;

/// <summary>
/// Tests for PowerScript language features.
/// Validates: variables, types, operators, control flow, functions, scoping, LINK, .NET interop
/// </summary>
[TestFixture]
public class LanguageFeatureTests : TestBase
{
    [Test]
    [TestCaseSource(nameof(GetFeatureScripts))]
    public void Feature_ExecutesCorrectly(string scriptPath)
    {
        string testName = Path.GetFileNameWithoutExtension(scriptPath);
        string expectedOutput = ParseExpectedOutput(scriptPath);

        TestContext.WriteLine($"Test: {testName}");
        TestContext.WriteLine($"Expected: {expectedOutput}");

        string actualOutput = ExecuteScriptFile(scriptPath);

        TestContext.WriteLine($"Actual: {actualOutput}");

        // EXACT match - no tolerance for differences
        Assert.That(actualOutput, Is.EqualTo(expectedOutput),
            $"Language feature test '{testName}' produced incorrect output");
    }

    private static IEnumerable<TestCaseData> GetFeatureScripts()
    {
        foreach (string scriptPath in GetTestScripts("features/scripts"))
        {
            string testName = Path.GetFileNameWithoutExtension(scriptPath);
            yield return new TestCaseData(scriptPath).SetName(testName);
        }
    }
}
