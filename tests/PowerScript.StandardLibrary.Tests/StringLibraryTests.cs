using NUnit.Framework;

namespace PowerScript.StandardLibrary.Tests;

[TestFixture]
public class StringLibraryTests : StandardLibraryTestBase
{
    private const string LibPath = "stdlib/String.ps";

    // ========================================================================
    // STRING PROPERTIES
    // ========================================================================

    [Test]
    public void STR_LENGTH_SimpleString_ReturnsCorrectLength()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_LENGTH(""hello"")
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("5"));
    }

    [Test]
    public void IS_EMPTY_EmptyString_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IS_EMPTY("""")
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    // ========================================================================
    // STRING TRANSFORMATIONS
    // ========================================================================

    [Test]
    public void STR_UPPER_LowercaseString_ReturnsUppercase()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_UPPER(""hello"")
            FLEX dummy = Console -> WriteLine(result)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("HELLO"));
    }

    [Test]
    public void STR_LOWER_UppercaseString_ReturnsLowercase()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_LOWER(""WORLD"")
            FLEX dummy = Console -> WriteLine(result)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("world"));
    }

    [Test]
    public void STR_REVERSE_SimpleString_ReturnsReversed()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_REVERSE(""abc"")
            FLEX dummy = Console -> WriteLine(result)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("cba"));
    }

    // ========================================================================
    // STRING TRIMMING
    // ========================================================================

    [Test]
    public void STR_TRIM_StringWithSpaces_RemovesSpaces()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_TRIM(""  hello  "")
            FLEX dummy = Console -> WriteLine(result)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("hello"));
    }

    // ========================================================================
    // STRING SEARCH
    // ========================================================================

    [Test]
    public void STR_CONTAINS_SubstringPresent_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_CONTAINS(""hello world"", ""wor"")
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void STR_STARTS_WITH_CorrectPrefix_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_STARTS_WITH(""hello"", ""hel"")
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void STR_ENDS_WITH_CorrectSuffix_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_ENDS_WITH(""hello"", ""lo"")
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    // ========================================================================
    // STRING MANIPULATION
    // ========================================================================

    [Test]
    public void STR_SUBSTRING_ValidRange_ReturnsSubstring()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_SUBSTRING(""hello"", 1, 3)
            FLEX dummy = Console -> WriteLine(result)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("ell"));
    }

    [Test]
    public void STR_REPLACE_FindAndReplace_ReplacesText()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_REPLACE(""hello"", ""ll"", ""yy"")
            FLEX dummy = Console -> WriteLine(result)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("heyyo"));
    }

    [Test]
    public void STR_INSERT_AtPosition_InsertsText()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = STR_INSERT(""helo"", 2, ""l"")
            FLEX dummy = Console -> WriteLine(result)
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
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
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
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("0"));
    }
}
