using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the OR logical operator (SQL-style).
///     Used in conditional expressions: IF a > b OR c < d { ... }
/// </summary>
public class OrKeywordToken : Token, IKeyWordToken
{
    public OrKeywordToken()
    {
    }

    public OrKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After OR, expect an identifier or value</summary>
    public override Type[] Expectations =>
    [
        typeof(IdentifierToken),
        typeof(ValueToken),
        typeof(StringLiteralToken)
    ];

    public override string KeyWord => "OR";
}