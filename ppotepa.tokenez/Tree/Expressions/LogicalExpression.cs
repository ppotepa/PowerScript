using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Expressions;

/// <summary>
/// Represents a logical operation expression using AND/OR operators.
/// SQL-style: a > b AND c < d, x == y OR z != w
/// </summary>
public class LogicalExpression : Expression
{
    /// <summary>Left-hand side expression (condition)</summary>
    public Expression Left { get; set; }

    /// <summary>The logical operator (AND, OR)</summary>
    public Token Operator { get; set; }

    /// <summary>Right-hand side expression (condition)</summary>
    public Expression Right { get; set; }

    public override string ExpressionType => "LogicalOperation";

    public LogicalExpression(Expression left, Token operatorToken, Expression right)
    {
        StartToken = left.StartToken;
        Left = left;
        Operator = operatorToken;
        Right = right;
    }

    public override string ToString() => $"LogicalExpression({Left} {Operator} {Right})";
}
