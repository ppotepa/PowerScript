using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the RANGE keyword for explicit range iteration.
///     Used in CYCLE RANGE FROM start TO end loops.
///     Example: CYCLE RANGE FROM 1 TO 10 { ... }
/// </summary>
public class RangeKeywordToken : Token, IKeyWordToken
{
    public RangeKeywordToken()
    {
    }

    public RangeKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After RANGE, expect FROM keyword</summary>
    public override Type[] Expectations => [typeof(FromKeywordToken)];

    public override string KeyWord => "RANGE";

    public override string ToString()
    {
        return $"RangeKeywordToken '{RawToken?.Text}'";
    }
}
