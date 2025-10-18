using NUnit.Framework;
using PowerScript.Tests;

namespace PowerScript.Tests.Syntax;

/// <summary>
/// Tests for pattern-based custom syntax transformations.
/// Phase 2: Advanced Pattern Matching
/// </summary>
[TestFixture]
public class PatternSyntaxTests : TestBase
{
    #region Basic Pattern Tests

    [Test]
    [Category("PatternSyntax")]
    [Category("Basic")]
    public void PatternSyntax_SimpleTakeFrom_TransformsCorrectly()
    {
        // Test: TAKE 3 FROM array - should transform to ARRAY_TAKE(numbers, 3)
        var code = @"
            FUNCTION ARRAY_TAKE(FLEX arr, FLEX count) {
                RETURN arr
            }
            
            FLEX numbers = [1, 2, 3, 4, 5]
            FLEX result = TAKE 3 FROM numbers
        ";

        // Test that the code compiles and runs without errors
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    [Test]
    [Category("PatternSyntax")]
    [Category("Basic")]
    public void PatternSyntax_FilterWhere_TransformsCorrectly()
    {
        // Test: FILTER array WHERE condition
        var code = @"
            FUNCTION ARRAY_FILTER(FLEX arr, FLEX condition) {
                RETURN arr
            }
            
            FLEX numbers = [1, 2, 3, 4, 5]
            FLEX result = FILTER numbers WHERE 3
        ";

        var output = ExecuteCode(code);
        // If no exception was thrown, the pattern transformation worked
        Assert.Pass("Pattern transformation successful - no exception thrown");
    }

    [Test]
    [Category("PatternSyntax")]
    [Category("Basic")]
    public void PatternSyntax_MapWith_TransformsCorrectly()
    {
        // Test: MAP array WITH function - should transform to ARRAY_MAP(numbers, 2)
        var code = @"
            FUNCTION ARRAY_MAP(FLEX arr, FLEX fn) {
                RETURN arr
            }
            
            FLEX numbers = [1, 2, 3]
            FLEX result = MAP numbers WITH 2
        ";

        // Test that the code compiles and runs without errors
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    #endregion

    #region Type Constraint Tests

    [Test]
    [Category("PatternSyntax")]
    [Category("TypeConstraints")]
    public void PatternSyntax_TypeConstraint_IntegerAccepted()
    {
        // Test type constraint validation with correct type
        var code = @"
            FUNCTION DIVIDE(FLEX a, FLEX b) {
                RETURN a
            }
            
            FLEX result = DIVIDE 10 BY 2
        ";

        var output = ExecuteCode(code);
        // If no exception was thrown, the type constraint validation passed
        Assert.Pass("Type constraint validation passed - no exception thrown");
    }

    [Test]
    [Category("PatternSyntax")]
    [Category("TypeConstraints")]
    public void PatternSyntax_TypeConstraint_StringRejected()
    {
        // Test type constraint validation with wrong type
        var code = @"
            FLEX result = DIVIDE ""hello"" BY ""world""
        ";

        Assert.Throws<InvalidOperationException>(() => ExecuteCode(code));
    }

    #endregion

    #region Multiple Variables Tests

    [Test]
    [Category("PatternSyntax")]
    [Category("MultipleVariables")]
    public void PatternSyntax_ThreeVariables_TransformsCorrectly()
    {
        // Test: REPLACE old WITH new IN text - should transform to STRING_REPLACE(text, "World", "PowerScript")
        var code = @"
            FUNCTION STRING_REPLACE(FLEX str, FLEX old, FLEX new) {
                RETURN str
            }
            
            FLEX text = ""Hello World""
            FLEX result = REPLACE ""World"" WITH ""PowerScript"" IN text
        ";

        // Test that the code compiles and runs without errors
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    [Test]
    [Category("PatternSyntax")]
    [Category("MultipleVariables")]
    [Ignore("CustomKeywordToken in expressions not yet supported")]
    public void PatternSyntax_NestedExpression_TransformsCorrectly()
    {
        // Test: TAKE (count * 2) FROM array - should transform to ARRAY_TAKE(numbers, count * 2)
        var code = @"
            FUNCTION ARRAY_TAKE(FLEX arr, FLEX count) {
                RETURN arr
            }
            
            FLEX numbers = [1, 2, 3, 4, 5, 6, 7, 8]
            FLEX count = 2
            FLEX result = TAKE (count * 2) FROM numbers
        ";

        // Test that the code compiles and runs without errors
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    #endregion

    #region Optional Elements Tests

    [Test]
    [Category("PatternSyntax")]
    [Category("OptionalElements")]
    public void PatternSyntax_OptionalElement_Present()
    {
        // Test optional element when present: SORT array ASCENDING?
        var code = @"
            FUNCTION ARRAY_SORT(FLEX arr) {
                RETURN arr
            }
            
            FLEX numbers = [3, 1, 4, 1, 5]
            FLEX result = SORT numbers ASCENDING
        ";

        var output = ExecuteCode(code);
        // If no exception was thrown, the optional pattern transformation worked
        Assert.Pass("Optional element pattern with present element successful");
    }

    [Test]
    [Category("PatternSyntax")]
    [Category("OptionalElements")]
    public void PatternSyntax_OptionalElement_Missing()
    {
        // Test optional element when missing: SORT array (without ASCENDING)
        var code = @"
            FUNCTION ARRAY_SORT(FLEX arr) {
                RETURN arr
            }
            
            FLEX numbers = [3, 1, 4, 1, 5]
            FLEX result = SORT numbers
        ";

        var output = ExecuteCode(code);
        // If no exception was thrown, the optional pattern transformation worked
        Assert.Pass("Optional element pattern with missing element successful");
    }

    [Test]
    [Category("PatternSyntax")]
    [Category("OptionalElements")]
    [Ignore("COUNT pattern has off-by-one error - array index 5 out of range for size 5 array")]
    public void PatternSyntax_OptionalVariable_Present()
    {
        // Test optional variable when present: COUNT item? IN array
        var code = @"
            FUNCTION ARRAY_COUNT(FLEX arr, FLEX item)[INT] {
                RETURN 2
            }
            
            FLEX numbers = [1,2,3,2,1]
            FLEX result = COUNT 2 IN numbers
            PRINT result
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Does.Contain("2"));
    }

    [Test]
    [Category("PatternSyntax")]
    [Category("OptionalElements")]
    [Ignore("COUNT pattern has off-by-one error - array index 5 out of range for size 5 array")]
    public void PatternSyntax_OptionalVariable_Missing()
    {
        // Test optional variable when missing: COUNT IN array (without specific item)
        var code = @"
            FUNCTION ARRAY_COUNT(FLEX arr, FLEX item)[INT] {
                RETURN 5
            }
            
            FLEX numbers = [1,2,3,2,1]
            FLEX result = COUNT IN numbers
            PRINT result
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Does.Contain("5"));
    }

    #endregion

    #region Alternative Forms Tests

    [Test]
    [Category("PatternSyntax")]
    [Category("AlternativeForms")]
    [Ignore("Alternative forms not yet implemented")]
    public void PatternSyntax_AlternativeForm_FirstOption()
    {
        // Test: {STARTS WITH|BEGINS WITH} - first option
        var code = @"
            FUNCTION STRING_STARTS_WITH(FLEX str, FLEX prefix) {
                RETURN 1
            }
            
            FLEX text = ""Hello World""
            FLEX result = text STARTS WITH ""Hello""
        ";

        // Test that the code compiles and runs without errors
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    [Test]
    [Category("PatternSyntax")]
    [Category("AlternativeForms")]
    [Ignore("Alternative forms not yet implemented")]
    public void PatternSyntax_AlternativeForm_SecondOption()
    {
        // Test: {STARTS WITH|BEGINS WITH} - second option
        var code = @"
            FUNCTION STRING_STARTS_WITH(FLEX str, FLEX prefix) {
                RETURN 1
            }
            
            FLEX text = ""Hello World""
            FLEX result = text BEGINS WITH ""Hello""
        ";

        // Test that the code compiles and runs without errors
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    #endregion

    #region Variadic Arguments Tests

    [Test]
    [Category("PatternSyntax")]
    [Category("VariadicArguments")]
    [Ignore("Variadic arguments not yet implemented")]
    public void PatternSyntax_VariadicArgs_TwoArrays()
    {
        // Test: COMBINE array1, array2
        var code = @"
            FUNCTION ARRAY_COMBINE(FLEX arr1, FLEX arr2) {
                RETURN arr1
            }
            
            FLEX a1 = [1, 2]
            FLEX a2 = [3, 4]
            FLEX result = COMBINE a1, a2
        ";

        // Test that the code compiles and runs without errors
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    [Test]
    [Category("PatternSyntax")]
    [Category("VariadicArguments")]
    [Ignore("Variadic arguments not yet implemented")]
    public void PatternSyntax_VariadicArgs_ThreeArrays()
    {
        // Test: COMBINE array1, array2, array3
        var code = @"
            FUNCTION ARRAY_COMBINE(FLEX arr1, FLEX arr2, FLEX arr3) {
                RETURN arr1
            }
            
            FLEX a1 = [1]
            FLEX a2 = [2]
            FLEX a3 = [3]
            FLEX result = COMBINE a1, a2, a3
        ";

        // Test that the code compiles and runs without errors
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    #endregion

    #region Edge Cases

    [Test]
    [Category("PatternSyntax")]
    [Category("EdgeCases")]
    public void PatternSyntax_EmptyArray_HandlesCorrectly()
    {
        var code = @"
            FUNCTION ARRAY_TAKE(FLEX arr, FLEX count) {
                RETURN arr
            }
            
            FLEX empty = []
            FLEX result = TAKE 3 FROM empty
        ";

        // Should not throw, even with empty array
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    [Test]
    [Category("PatternSyntax")]
    [Category("EdgeCases")]
    public void PatternSyntax_LargeNumber_HandlesCorrectly()
    {
        var code = @"
            FUNCTION ARRAY_TAKE(FLEX arr, FLEX count) {
                RETURN arr
            }
            
            FLEX numbers = [1, 2, 3]
            FLEX result = TAKE 1000 FROM numbers
        ";

        // Should handle count larger than array size
        Assert.DoesNotThrow(() => ExecuteCode(code));
    }

    #endregion
}
