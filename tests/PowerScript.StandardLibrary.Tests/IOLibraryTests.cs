using NUnit.Framework;

namespace PowerScript.StandardLibrary.Tests;

[TestFixture]
public class IOLibraryTests : StandardLibraryTestBase
{
    private const string LibPath = "stdlib/IO.ps";

    // ========================================================================
    // BASIC OUTPUT
    // ========================================================================

    [Test]
    public void OUT_Number_PrintsWithoutNewline()
    {
        var code = $@"
            LINK ""{LibPath}""
            
            FLEX dummy = OUT(42)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("42"));
    }

    [Test]
    public void OUTLN_Number_PrintsWithNewline()
    {
        var code = $@"
            LINK ""{LibPath}""
            
            FLEX dummy = OUTLN(42)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("42"));
    }

    [Test]
    public void OUT_STR_String_PrintsString()
    {
        var code = $@"
            LINK ""{LibPath}""
            
            FLEX dummy = OUT_STR(""Hello"")
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("Hello"));
    }

    [Test]
    public void OUT_NUM_Number_PrintsNumber()
    {
        var code = $@"
            LINK ""{LibPath}""
            
            FLEX dummy = OUT_NUM(123)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("123"));
    }

    // ========================================================================
    // MULTI-VALUE OUTPUT
    // ========================================================================

    [Test]
    public void OUT_MULTI_TwoNumbers_PrintsBoth()
    {
        var code = $@"
            LINK ""{LibPath}""
            
            FLEX dummy = OUT_MULTI(10, 20)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1020"));
    }

    [Test]
    public void OUT_THREE_ThreeValues_PrintsAll()
    {
        var code = $@"
            LINK ""{LibPath}""
            
            FLEX dummy = OUT_THREE(1, 2, 3)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("123"));
    }

    // ========================================================================
    // SPECIAL CHARACTERS
    // ========================================================================

    [Test]
    public void SPACE_NoArgs_PrintsSpace()
    {
        var code = $@"
            LINK ""{LibPath}""
            
            FLEX d1 = OUT(42)
            FLEX d2 = SPACE()
            FLEX d3 = OUT(99)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("42 99"));
    }

    // ========================================================================
    // LABELED OUTPUT
    // ========================================================================

    [Test]
    public void PRINT_LABELED_LabelAndValue_PrintsBoth()
    {
        var code = $@"
            LINK ""{LibPath}""
            
            FLEX dummy = PRINT_LABELED(""Result"", 42)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("Result: 42"));
    }

    // ========================================================================
    // LEGACY WRAPPERS
    // ========================================================================

    [Test]
    public void PRINT_Number_PrintsNumber()
    {
        var code = $@"
            LINK ""{LibPath}""
            
            FLEX dummy = PRINT(42)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("42"));
    }

    [Test]
    public void PRINT_STRING_String_PrintsString()
    {
        var code = $@"
            LINK ""{LibPath}""
            
            FLEX dummy = PRINT_STRING(""test"")
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("test"));
    }

    [Test]
    public void PRINT_INLINE_Number_PrintsInline()
    {
        var code = $@"
            LINK ""{LibPath}""
            
            FLEX d1 = PRINT_INLINE(5)
            FLEX d2 = PRINT_INLINE(10)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("510"));
    }
}
