using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Keywords;

/// <summary>
/// Token representing the OR logical operator (SQL-style).
/// Used in conditional expressions: IF a > b OR c < d { ... }
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
    public override Type[] Expectations => [
        typeof(IdentifierToken),
        typeof(ValueToken),
        typeof(StringLiteralToken)
    ];

    public override string KeyWord => "OR";
}
