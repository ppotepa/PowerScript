using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the AND logical operator (SQL-style).
///     Used in conditional expressions: IF a > b AND c < d { ... }
/// </summary>
public class AndKeywordToken : Token, IKeyWordToken
{
    public AndKeywordToken()
    {
    }

    public AndKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After AND, expect an identifier or value</summary>
    public override Type[] Expectations =>
    [
        typeof(IdentifierToken),
        typeof(ValueToken),
        typeof(StringLiteralToken)
    ];

    public override string KeyWord => "AND";
}