using NUnit.Framework;
using PowerScript.Tests;

namespace PowerScript.Tests.Syntax;

/// <summary>
/// Tests for type constraint validation in custom syntax patterns.
/// Phase 2: Type System Integration
/// </summary>
[TestFixture]
public class TypeConstraintTests : TestBase
{
    #region Basic Type Constraints

    [Test]
    [Category("TypeConstraints")]
    [Category("Integer")]
    public void TypeConstraint_IntParameter_AcceptsInteger()
    {
        var code = @"
            FUNCTION MULTIPLY(FLEX a, FLEX b)[FLEX] {
                RETURN a * b
            }
            
            FLEX result = MULTIPLY 5 BY 3
            PRINT(result)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Does.Contain("15"));
    }

    [Test]
    [Category("TypeConstraints")]
    [Category("String")]
    public void TypeConstraint_StringParameter_AcceptsString()
    {
        var code = @"
            FUNCTION STRING_UPPER(FLEX str)[FLEX] {
                RETURN str
            }
            
            FLEX result = UPPER ""hello""
            PRINT(result)
        ";

        // This test will work once we implement UPPER pattern
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    [Test]
    [Category("TypeConstraints")]
    [Category("Array")]
    public void TypeConstraint_ArrayParameter_AcceptsArray()
    {
        var code = @"
            FUNCTION ARRAY_LENGTH(FLEX arr)[INT] {
                RETURN 5
            }
            
            FLEX numbers = [1,2,3,4,5]
            FLEX result = ARRAY_LENGTH(numbers)
        ";

        // This test will work once we implement LENGTH OF pattern
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    #endregion

    #region Type Mismatch Tests

    [Test]
    [Category("TypeConstraints")]
    [Category("TypeMismatch")]
    [Ignore("Type validation not yet implemented")]
    public void TypeConstraint_IntExpected_StringProvided_ThrowsError()
    {
        var code = @"
            FLEX result = MULTIPLY ""hello"" BY ""world""
        ";

        Assert.Throws<InvalidOperationException>(() => ExecuteCode(code));
    }

    [Test]
    [Category("TypeConstraints")]
    [Category("TypeMismatch")]
    [Ignore("Type validation not yet implemented")]
    public void TypeConstraint_ArrayExpected_IntProvided_ThrowsError()
    {
        var code = @"
            FLEX result = TAKE 3 FROM 42
        ";

        Assert.Throws<InvalidOperationException>(() => ExecuteCode(code));
    }

    [Test]
    [Category("TypeConstraints")]
    [Category("TypeMismatch")]
    [Ignore("Type validation not yet implemented")]
    public void TypeConstraint_StringExpected_ArrayProvided_ThrowsError()
    {
        var code = @"
            FLEX text = [1, 2, 3]
            FLEX result = text STARTS WITH ""H""
        ";

        Assert.Throws<InvalidOperationException>(() => ExecuteCode(code));
    }

    #endregion

    #region Type Conversion Tests

    [Test]
    [Category("TypeConstraints")]
    [Category("TypeConversion")]
    [Ignore("Type conversion not yet implemented")]
    public void TypeConstraint_IntToFloat_ConvertsAutomatically()
    {
        var code = @"
            FUNCTION DIVIDE_FLOAT(FLEX a, FLEX b)[FLEX] {
                RETURN a
            }
            
            FLEX result = DIVIDE 10 BY 3.0
            PRINT(result)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Does.Contain("10"));
    }

    [Test]
    [Category("TypeConstraints")]
    [Category("TypeConversion")]
    [Ignore("Type conversion not yet implemented")]
    public void TypeConstraint_StringToInt_ConvertsWithWarning()
    {
        var code = @"
            FLEX result = MULTIPLY ""5"" BY 3
            PRINT(result)
        ";

        var output = ExecuteCode(code);
        // Should convert "5" to 5 and multiply
        Assert.That(output, Does.Contain("15"));
    }

    #endregion

    #region Complex Type Constraints

    [Test]
    [Category("TypeConstraints")]
    [Category("ComplexTypes")]
    [Ignore("Generic type constraints not yet implemented")]
    public void TypeConstraint_GenericArray_AcceptsIntArray()
    {
        var code = @"
            FLEX numbers = [1, 2, 3, 4, 5]
            FLEX result = TAKE 3 FROM numbers
            PRINT(result)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Does.Contain("1"));
    }

    [Test]
    [Category("TypeConstraints")]
    [Category("ComplexTypes")]
    [Ignore("Generic type constraints not yet implemented")]
    public void TypeConstraint_GenericArray_RejectsWrongType()
    {
        var code = @"
            FLEX strings = [""a"", ""b"", ""c""]
            FLEX result = SUM OF strings
        ";

        Assert.Throws<InvalidOperationException>(() => ExecuteCode(code));
    }

    #endregion

    #region FLEX Type Tests

    [Test]
    [Category("TypeConstraints")]
    [Category("FlexType")]
    public void TypeConstraint_FlexType_AcceptsAnyType()
    {
        var code = @"
            FUNCTION ECHO(FLEX value)[FLEX] {
                RETURN value
            }
            
            FLEX result1 = ECHO(42)
            FLEX result2 = ECHO(""hello"")
            PRINT(result1)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Does.Contain("42"));
    }

    #endregion
}
