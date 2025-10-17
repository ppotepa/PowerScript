using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Operators;

/// <summary>
///     Token representing the custom syntax operator ::.
///     Used for custom syntax extensions loaded from .psx files.
///     Example: "array::Sort()" transforms to "ARRAY_SORT(array)"
/// </summary>
public class CustomSyntaxOperatorToken : Token
{
    public CustomSyntaxOperatorToken()
    {
    }

    public CustomSyntaxOperatorToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After ::, expect the method name identifier</summary>
    public override Type[] Expectations => [typeof(IdentifierToken)];
}