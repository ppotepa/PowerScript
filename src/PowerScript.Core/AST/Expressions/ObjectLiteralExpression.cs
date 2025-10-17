namespace PowerScript.Core.AST.Expressions;

/// <summary>
/// Represents an object literal expression.
/// Example: {name = "John", age = 30} or {x = 1, y = 2} as Point
/// </summary>
public class ObjectLiteralExpression : Expression
{
    /// <summary>Dictionary of property names to their value expressions</summary>
    public Dictionary<string, Expression> Properties { get; set; }

    /// <summary>Optional type name for type annotation (e.g., "Person")</summary>
    public string? TypeName { get; set; }

    /// <summary>Whether this is a strict type (marked with !)</summary>
    public bool IsStrict { get; set; }

    public ObjectLiteralExpression(Dictionary<string, Expression> properties, string? typeName = null, bool isStrict = false)
    {
        Properties = properties;
        TypeName = typeName;
        IsStrict = isStrict;
    }

    public override string ExpressionType => "ObjectLiteral";

    public override string ToString()
    {
        var props = string.Join(", ", Properties.Select(p => $"{p.Key} = {p.Value}"));
        var typeAnnotation = TypeName != null ? $" as {TypeName}{(IsStrict ? "!" : "")}" : "";
        return $"{{{props}}}{typeAnnotation}";
    }
}
