using NUnit.Framework;

namespace PowerScript.StandardLibrary.Tests;

[TestFixture]
public class ValidationLibraryTests : StandardLibraryTestBase
{
    private const string LibPath = "stdlib/Validation.ps";

    // ========================================================================
    // NUMERIC VALIDATION
    // ========================================================================

    [Test]
    public void IS_POSITIVE_PositiveNumber_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IS_POSITIVE(5)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void IS_NEGATIVE_NegativeNumber_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IS_NEGATIVE(0 - 5)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void IS_ZERO_Zero_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IS_ZERO(0)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    // ========================================================================
    // RANGE VALIDATION
    // ========================================================================

    [Test]
    public void IN_RANGE_NumberInRange_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IN_RANGE(5, 1, 10)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void IN_RANGE_NumberOutOfRange_ReturnsZero()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IN_RANGE(15, 1, 10)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("0"));
    }

    [Test]
    public void BETWEEN_INCLUSIVE_NumberAtBoundary_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = BETWEEN_INCLUSIVE(10, 10, 20)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    // ========================================================================
    // DIVISIBILITY TESTS
    // ========================================================================

    [Test]
    public void IS_DIVISIBLE_EvenlyDivisible_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IS_DIVISIBLE(10, 5)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void IS_MULTIPLE_NumberIsMultiple_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IS_MULTIPLE(15, 5)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    // ========================================================================
    // CHARACTER VALIDATION
    // ========================================================================

    [Test]
    public void IS_ALPHA_Letter_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IS_ALPHA(""a"")
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void IS_DIGIT_Digit_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IS_DIGIT(""5"")
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void IS_UPPER_UppercaseLetter_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IS_UPPER(""A"")
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void IS_LOWER_LowercaseLetter_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IS_LOWER(""a"")
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    // ========================================================================
    // STRING VALIDATION
    // ========================================================================

    [Test]
    public void VALIDATE_NOT_EMPTY_NonEmptyString_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = VALIDATE_NOT_EMPTY(""hello"")
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void VALIDATE_LENGTH_CorrectLength_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = VALIDATE_LENGTH(""hello"", 5)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void VALIDATE_MIN_LENGTH_MeetsMinimum_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = VALIDATE_MIN_LENGTH(""hello"", 3)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    // ========================================================================
    // NUMERIC RANGE VALIDATION
    // ========================================================================

    [Test]
    public void VALIDATE_RANGE_ValueInRange_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = VALIDATE_RANGE(50, 1, 100)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void VALIDATE_MIN_ValueAboveMin_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = VALIDATE_MIN(10, 5)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }

    [Test]
    public void VALIDATE_MAX_ValueBelowMax_ReturnsOne()
    {
        var code = $@"
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = VALIDATE_MAX(10, 20)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }
}
