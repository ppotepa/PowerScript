namespace ppotepa.tokenez.Tree.Expressions;

/// <summary>
///     Represents an array literal expression.
///     Syntax: [value1, value2, value3, ...]
///     Example: FLEX arr = [1, 3, 4, 5, 6]
/// </summary>
public class ArrayLiteralExpression : Expression
{
    public ArrayLiteralExpression(List<Expression> elements)
    {
        Elements = elements;
    }

    public ArrayLiteralExpression()
    {
        Elements = [];
    }

    public List<Expression> Elements { get; }

    public override string ExpressionType { get; set; } = "ArrayLiteral";
}