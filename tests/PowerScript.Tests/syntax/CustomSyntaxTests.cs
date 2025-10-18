using NUnit.Framework;

namespace PowerScript.Tests.Syntax;

/// <summary>
/// Tests for custom syntax extensions (.psx files).
/// Tests operator-based syntax (::), pattern-based syntax, and chaining.
/// </summary>
[TestFixture]
public class CustomSyntaxTests : TestBase
{
    private const string ScriptsFolder = "syntax/scripts";

    [Test]
    [TestCase("array_operator_syntax.ps")]
    [TestCase("string_operator_syntax.ps")]
    [TestCase("chaining_syntax.ps")]
    [TestCase("pattern_syntax.ps")]
    [TestCase("string_mixed_syntax.ps")]
    public void CustomSyntax_ProducesCorrectOutput(string scriptPath)
    {
        // Arrange
        string baseDirectory = TestContext.CurrentContext.TestDirectory;
        string fullPath = Path.Combine(baseDirectory, ScriptsFolder, scriptPath);

        string expectedOutput = ParseExpectedOutput(fullPath);
        string actualOutput = ExecuteScriptFile(fullPath);

        // Act & Assert
        Console.WriteLine($"Test: {Path.GetFileNameWithoutExtension(scriptPath)}");
        Console.WriteLine($"Expected: {expectedOutput}");
        Console.WriteLine($"Actual: {actualOutput}");
        Console.WriteLine();

        Assert.That(actualOutput, Is.EqualTo(expectedOutput),
            $"Custom syntax test '{scriptPath}' produced incorrect output");
    }
}
