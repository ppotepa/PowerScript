namespace PowerScript.Core.AST.Expressions;

/// <summary>
///     Represents a .NET member access expression using the arrow operator (->).
///     Example: str -> Length or person -> Speak()
///     This allows PowerScript to access .NET properties and methods.
/// </summary>
public class NetMemberAccessExpression : Expression
{
    public NetMemberAccessExpression(Expression target, string memberName, List<Expression>? arguments = null)
    {
        ExpressionType = "NetMemberAccess";
        Target = target;
        MemberName = memberName;
        Arguments = arguments ?? [];
        IsMethodCall = arguments != null;
    }

    /// <summary>The object/expression on the left side of the arrow</summary>
    public Expression Target { get; set; }

    /// <summary>The member name (property or method) on the right side of the arrow</summary>
    public string MemberName { get; set; }

    /// <summary>Arguments for method calls (null for property access)</summary>
    public List<Expression> Arguments { get; set; }

    /// <summary>True if this is a method call, false if it's a property access</summary>
    public bool IsMethodCall { get; set; }

    public override string ToString()
    {
        if (IsMethodCall)
        {
            string args = string.Join(", ", Arguments.Select(a => a.ToString()));
            return $"{Target} -> {MemberName}({args})";
        }

        return $"{Target} -> {MemberName}";
    }
}
