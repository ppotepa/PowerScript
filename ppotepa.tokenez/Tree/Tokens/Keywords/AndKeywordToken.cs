using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Keywords;

/// <summary>
/// Token representing the AND logical operator (SQL-style).
/// Used in conditional expressions: IF a > b AND c < d { ... }
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
    public override Type[] Expectations => [
        typeof(IdentifierToken),
        typeof(ValueToken),
        typeof(StringLiteralToken)
    ];

    public override string KeyWord => "AND";
}
