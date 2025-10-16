using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the FROM keyword for range-based loops.
///     Used in CYCLE RANGE FROM start TO end loops.
///     Example: CYCLE RANGE FROM 1 TO 10 { ... }
/// </summary>
public class FromKeywordToken : Token, IKeyWordToken
{
    public FromKeywordToken()
    {
    }

    public FromKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After FROM, expect the start value (identifier or value)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];

    public override string KeyWord => "FROM";

    public override string ToString()
    {
        return $"FromKeywordToken '{RawToken?.Text}'";
    }
}
