using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Operators;

/// <summary>
///     Token representing the assignment operator (=).
///     Used in variable declarations and assignments.
///     Example: "VAR x = 10" or "x = 20"
/// </summary>
public class EqualsToken : Token
{
    public EqualsToken()
    {
    }

    public EqualsToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After =, expect a value, identifier, or expression</summary>
    public override Type[] Expectations =>
    [
        typeof(ValueToken),
        typeof(IdentifierToken),
        typeof(StringLiteralToken)
    ];
}