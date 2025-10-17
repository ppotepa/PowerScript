using NUnit.Framework;

namespace PowerScript.StandardLibrary.Tests;

[TestFixture]
public class StringLibraryTests : StandardLibraryTestBase
{
    private const string LibPath = "stdlib/String.ps";

    // ========================================================================
    // STRING MANIPULATION
    // ========================================================================
    // NOTE: Basic string operations (STR_LENGTH, IS_EMPTY, STR_UPPER, STR_LOWER,
    // STR_REVERSE, STR_TRIM, STR_CONTAINS, STR_STARTS_WITH, STR_ENDS_WITH,
    // STR_SUBSTRING, STR_REPLACE) have been migrated to PowerScript test files
    // in scripts/stdlib/stdlib_3_*.ps

    [Test]
    public void STR_INSERT_AtPosition_InsertsText()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_INSERT(""helo"", 2, ""l"")
             #Console->WriteLine(result)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("hello"));
    }

    // ========================================================================
    // STRING COMPARISON
    // ========================================================================

    [Test]
    public void STR_EQUALS_SameStrings_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_EQUALS(""test"", ""test"")
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void STR_EQUALS_DifferentStrings_ReturnsZero()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_EQUALS(""test"", ""hello"")
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("0"));
    }
}



