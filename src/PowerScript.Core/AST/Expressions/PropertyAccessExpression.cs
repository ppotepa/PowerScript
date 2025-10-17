namespace PowerScript.Core.AST.Expressions;

/// <summary>
/// Represents a property access expression.
/// Example: person.name or obj.value
/// </summary>
public class PropertyAccessExpression : Expression
{
    /// <summary>The target expression (what we're accessing the property on)</summary>
    public Expression Target { get; set; }

    /// <summary>The name of the property being accessed</summary>
    public string PropertyName { get; set; }

    public PropertyAccessExpression(Expression target, string propertyName)
    {
        Target = target;
        PropertyName = propertyName;
    }

    public override string ExpressionType => "PropertyAccess";

    public override string ToString()
    {
        return $"{Target}.{PropertyName}";
    }
}
