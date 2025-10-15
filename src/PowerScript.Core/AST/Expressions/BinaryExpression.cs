using PowerScript.Core.Syntax.Tokens.Base;

namespace PowerScript.Core.AST.Expressions;

/// <summary>
///     Represents a binary operation expression (e.g., a + b, x * y).
///     Contains left operand, operator, and right operand.
/// </summary>
public class BinaryExpression : Expression
{
    public BinaryExpression(Expression left, Token operatorToken, Expression right)
    {
        StartToken = left.StartToken;
        Left = left;
        Operator = operatorToken;
        Right = right;
    }

    /// <summary>Left-hand side expression</summary>
    public Expression Left { get; set; }

    /// <summary>The operator token (+, -, *, /)</summary>
    public Token Operator { get; set; }

    /// <summary>Right-hand side expression</summary>
    public Expression Right { get; set; }

    public override string ExpressionType { get; set; } = "BinaryOperation";
}