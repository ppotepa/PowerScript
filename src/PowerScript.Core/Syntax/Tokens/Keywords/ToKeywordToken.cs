using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the TO keyword for range-based loops.
///     Used in CYCLE start TO end loops.
///     Example: CYCLE -5 TO 5 { ... }
/// </summary>
public class ToKeywordToken : Token, IKeyWordToken
{
    public ToKeywordToken()
    {
    }

    public ToKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After TO, expect the end value (identifier or value)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];

    public override string KeyWord => "TO";

    public override string ToString()
    {
        return $"ToKeywordToken '{RawToken?.Text}'";
    }
}
