using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the WHILE keyword for while-style loops.
///     Used in CYCLE WHILE condition loops.
///     Example: CYCLE WHILE counter < 10 { ... }
/// </summary>
public class WhileKeywordToken : Token, IKeyWordToken
{
    public WhileKeywordToken()
    {
    }

    public WhileKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After WHILE, expect a condition expression (identifier or value)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];

    public override string KeyWord => "WHILE";

    public override string ToString()
    {
        return $"WhileKeywordToken '{RawToken?.Text}'";
    }
}
