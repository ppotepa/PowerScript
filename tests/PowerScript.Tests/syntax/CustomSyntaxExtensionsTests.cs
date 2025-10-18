using System;
using System.IO;
using NUnit.Framework;
using PowerScript.Runtime;

namespace PowerScript.Tests.Syntax;

/// <summary>
/// Unit tests for custom syntax features (.psx files)
/// Tests both operator-based (::) and pattern-based custom syntax
/// </summary>
[TestFixture]
public class CustomSyntaxExtensionsTests : TestBase
{
    private const string ExamplesFolder = "syntax/examples";

    [Test]
    [Ignore("Custom syntax not yet implemented")]
    public void Test_01_ArrayOperators_ShouldWork()
    {
        // Test array::Sort(), array::First(), array::Sum(), etc.
        // Expected: DEMO_ARRAY_OPERATORS() returns 1
        // Will pass when :: operator is implemented
        Assert.Ignore("Example script exists at stdlib/syntax/examples/01_array_operators.ps");
    }

    [Test]
    [Ignore("Custom syntax not yet implemented")]
    public void Test_02_ArrayPatterns_ShouldWork()
    {
        // Test FILTER, MAP, TAKE FROM, SKIP IN patterns
        // Expected: DEMO_ARRAY_PATTERNS() returns 1
        // Will pass when pattern syntax is implemented
        Assert.Ignore("Example script exists at stdlib/syntax/examples/02_array_patterns.ps");
    }

    [Test]
    [Ignore("Custom syntax not yet implemented")]
    public void Test_03_StringOperators_ShouldWork()
    {
        // Test string::ToUpper(), string::Trim(), string::Split(), etc.
        // Expected: DEMO_STRING_OPERATORS() returns 1
        Assert.Ignore("Example script exists at stdlib/syntax/examples/03_string_operators.ps");
    }

    [Test]
    [Ignore("Custom syntax not yet implemented")]
    public void Test_04_StringPatterns_ShouldWork()
    {
        // Test JOIN WITH, REPEAT TIMES patterns
        // Expected: DEMO_STRING_PATTERNS() returns 1
        Assert.Ignore("Example script exists at stdlib/syntax/examples/04_string_patterns.ps");
    }

    [Test]
    [Ignore("Custom syntax not yet implemented")]
    public void Test_05_ObjectSyntax_ShouldWork()
    {
        // Test object::Props(), object::HasProp(), FILTER Properties patterns
        // Expected: DEMO_OBJECT_SYNTAX() returns 1
        Assert.Ignore("Example script exists at stdlib/syntax/examples/05_object_syntax.ps");
    }

    [Test]
    [Ignore("Custom syntax not yet implemented")]
    public void Test_06_ChainedOperations_ShouldWork()
    {
        // Test chaining: array::Sort()::Reverse()::First()
        // Expected: DEMO_CHAINED_OPERATIONS() returns 1
        Assert.Ignore("Example script exists at stdlib/syntax/examples/06_chained_operations.ps");
    }

    [Test]
    [Ignore("Custom syntax not yet implemented")]
    public void Test_07_CollectionQueries_ShouldWork()
    {
        // Test :> WHERE, :> ORDER_BY, :> GROUP_BY, :> SELECT operators
        // Expected: DEMO_COLLECTION_QUERIES() returns 1
        Assert.Ignore("Example script exists at stdlib/syntax/examples/07_collection_queries.ps");
    }

    [Test]
    [Ignore("Custom syntax not yet implemented")]
    public void Test_08_AdvancedArrayOps_ShouldWork()
    {
        // Test +, -, &, [..], [?] operators
        // Expected: DEMO_ADVANCED_ARRAY_OPS() returns 1
        Assert.Ignore("Example script exists at stdlib/syntax/examples/08_advanced_array_ops.ps");
    }

    [Test]
    [Ignore("Custom syntax not yet implemented")]
    public void Test_09_PipelineOps_ShouldWork()
    {
        // Test |> and => operators
        // Expected: DEMO_PIPELINE_OPS() returns 1
        Assert.Ignore("Example script exists at stdlib/syntax/examples/09_pipeline_ops.ps");
    }

    [Test]
    [Ignore("Custom syntax not yet implemented")]
    public void Test_10_TypeObjectOps_ShouldWork()
    {
        // Test AS, ?>, .., ??, WITH operators
        // Expected: DEMO_TYPE_AND_OBJECT_OPS() returns 1
        Assert.Ignore("Example script exists at stdlib/syntax/examples/10_type_object_ops.ps");
    }

    [Test]
    [Ignore("Operator not yet implemented")]
    public void Test_DoubleColonOperator_ShouldBeRecognized()
    {
        // Test that :: operator is recognized as a token
        // Expected: Should tokenize as: IDENTIFIER(array), DOUBLE_COLON(::), IDENTIFIER(Sort), LPAREN, RPAREN
        Assert.Ignore(":: operator not yet added to lexer");
    }

    [Test]
    [Ignore("Operator not yet implemented")]
    public void Test_PipelineOperator_ShouldBeRecognized()
    {
        // Test that |> operator is recognized as a token
        // Expected: Should tokenize as: IDENTIFIER(data), PIPE_RIGHT(|>), IDENTIFIER(TRANSFORM)
        Assert.Ignore("|> operator not yet added to lexer");
    }

    [Test]
    [Ignore("Operator not yet implemented")]
    public void Test_QueryOperator_ShouldBeRecognized()
    {
        // Test that :> operator is recognized as a token
        // Expected: Should tokenize as: IDENTIFIER, QUERY_OP(:>), IDENTIFIER(WHERE), ...
        Assert.Ignore(":> operator not yet added to lexer");
    }

    [Test]
    [Ignore("Operator not yet implemented")]
    public void Test_NullCoalescing_ShouldWork()
    {
        // Test that ?? operator works correctly
        // Expected: result should be 42
        Assert.Ignore("?? operator not yet implemented");
    }

    [Test]
    [Ignore("Operator not yet implemented")]
    public void Test_ArrayConcatenation_ShouldWork()
    {
        // Test that + operator concatenates arrays
        // Expected: result should be [1, 2, 3, 4]
        Assert.Ignore("Array + operator not yet implemented");
    }

    [Test]
    [Ignore("Operator not yet implemented")]
    public void Test_ArrayDifference_ShouldWork()
    {
        // Test that - operator computes set difference
        // Expected: result should be [1]
        Assert.Ignore("Array - operator not yet implemented");
    }

    [Test]
    [Ignore("Operator not yet implemented")]
    public void Test_ArrayIntersection_ShouldWork()
    {
        // Test that & operator computes set intersection
        // Expected: result should be [2, 3]
        Assert.Ignore("Array & operator not yet implemented");
    }
}

