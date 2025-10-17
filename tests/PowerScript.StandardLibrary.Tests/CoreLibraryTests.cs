using NUnit.Framework;

namespace PowerScript.StandardLibrary.Tests;

[TestFixture]
public class CoreLibraryTests : StandardLibraryTestBase
{
    private const string LibPath = "stdlib/Core.ps";

    // ========================================================================
    // ARITHMETIC OPERATIONS
    // ========================================================================
    // NOTE: Basic arithmetic operations (ADD, SUB, MULT, DIV, MOD) have been
    // migrated to PowerScript test files in scripts/stdlib/stdlib_6_*.ps

    // ========================================================================
    // COMPARISON OPERATIONS
    // ========================================================================

    [Test]
    public void EQUALS_SameNumbers_ReturnsOne()
    {
        string code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = EQUALS(5, 5)
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        string output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void EQUALS_DifferentNumbers_ReturnsZero()
    {
        string code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = EQUALS(5, 3)
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        string output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("0"));
    }

    [Test]
    public void GREATER_THAN_FirstLarger_ReturnsOne()
    {
        string code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = GREATER_THAN(10, 5)
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        string output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void LESS_THAN_FirstSmaller_ReturnsOne()
    {
        string code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = LESS_THAN(3, 10)
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        string output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    // ========================================================================
    // MIN/MAX OPERATIONS
    // ========================================================================

    [Test]
    public void MAX_TwoNumbers_ReturnsLarger()
    {
        string code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = MAX(15, 23)
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        string output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("23"));
    }

    [Test]
    public void MIN_TwoNumbers_ReturnsSmaller()
    {
        string code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = MIN(15, 23)
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        string output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("15"));
    }

    // ========================================================================
    // SIGN AND ABSOLUTE VALUE
    // ========================================================================

    [Test]
    public void ABS_PositiveNumber_ReturnsSame()
    {
        string code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = ABS(42)
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        string output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("42"));
    }

    [Test]
    public void SIGN_PositiveNumber_ReturnsOne()
    {
        string code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = SIGN(42)
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        string output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void SIGN_Zero_ReturnsZero()
    {
        string code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = SIGN(0)
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        string output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("0"));
    }

    // ========================================================================
    // NESTED FUNCTION CALLS (Test Bug #3 fix)
    // ========================================================================

    [Test]
    public void ADD_WithNestedMULT_CalculatesCorrectly()
    {
        string code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = ADD(MULT(3, 4), 5)
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        string output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("17"));
    }

    [Test]
    public void MAX_WithNestedADD_CalculatesCorrectly()
    {
        string code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = MAX(ADD(2, 3), ADD(1, 5))
            FLEX str = #result->ToString()
             #Console->WriteLine(str)
        ";

        string output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("6"));
    }
}




