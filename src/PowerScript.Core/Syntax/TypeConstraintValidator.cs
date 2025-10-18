using PowerScript.Core.AST.Expressions;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax;

/// <summary>
/// Service for validating type constraints in custom syntax patterns.
/// </summary>
public static class TypeConstraintValidator
{
    /// <summary>
    /// Validates that an expression matches the expected type constraint.
    /// </summary>
    /// <param name="expression">The expression to validate</param>
    /// <param name="expectedType">The expected type (INT, STRING, ARRAY, etc.)</param>
    /// <param name="variableName">The variable name for error messages</param>
    /// <returns>True if validation passes</returns>
    public static bool ValidateTypeConstraint(Expression expression, string expectedType, string variableName)
    {
        // If no type constraint specified, accept anything
        if (string.IsNullOrEmpty(expectedType))
            return true;

        // FLEX type accepts anything
        if (expectedType == "FLEX")
            return true;

        // Get the actual type of the expression
        string actualType = InferExpressionType(expression);

        // Check if types match
        if (actualType == expectedType)
            return true;

        // Check if types are compatible (e.g., INT can be used as FLOAT)
        if (AreTypesCompatible(actualType, expectedType))
            return true;

        // Type mismatch
        throw new InvalidOperationException(
            $"Type mismatch for variable '${variableName}': expected {expectedType}, got {actualType}");
    }

    /// <summary>
    /// Infers the type of an expression.
    /// </summary>
    private static string InferExpressionType(Expression expression)
    {
        return expression switch
        {
            LiteralExpression literal => InferLiteralType(literal),
            StringLiteralExpression => "STRING",
            ArrayLiteralExpression => "ARRAY",
            IdentifierExpression => "FLEX", // Can't determine type without runtime info
            BinaryExpression => "FLEX", // Depends on operands
            FunctionCallExpression => "FLEX", // Depends on function return type
            _ => "FLEX"
        };
    }

    /// <summary>
    /// Infers the type of a literal expression.
    /// </summary>
    private static string InferLiteralType(LiteralExpression literal)
    {
        var token = literal.Value;

        if (token is ValueToken valueToken)
        {
            // Check if it's an integer
            if (int.TryParse(valueToken.ToString(), out _))
                return "INT";

            // Check if it's a float
            if (double.TryParse(valueToken.ToString(), out _))
                return "FLOAT";
        }

        if (token is DecimalToken)
            return "FLOAT";

        return "FLEX";
    }

    /// <summary>
    /// Checks if two types are compatible (e.g., INT can be used as FLOAT).
    /// </summary>
    private static bool AreTypesCompatible(string actualType, string expectedType)
    {
        // INT can be used as FLOAT
        if (actualType == "INT" && expectedType == "FLOAT")
            return true;

        // INT can be used as NUMBER
        if (actualType == "INT" && expectedType == "NUMBER")
            return true;

        // FLOAT can be used as NUMBER
        if (actualType == "FLOAT" && expectedType == "NUMBER")
            return true;

        // Any type can be used as FLEX
        if (expectedType == "FLEX")
            return true;

        return false;
    }

    /// <summary>
    /// Gets a friendly name for a type.
    /// </summary>
    public static string GetTypeName(string type)
    {
        return type switch
        {
            "INT" => "integer",
            "FLOAT" => "floating-point number",
            "STRING" => "string",
            "BOOL" => "boolean",
            "ARRAY" => "array",
            "MAP" => "map",
            "OBJECT" => "object",
            "FLEX" => "any type",
            _ => type.ToLower()
        };
    }
}
