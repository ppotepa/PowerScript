using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the ELEMENTS keyword for explicit collection iteration.
///     Used in CYCLE ELEMENTS OF collection loops.
///     Example: CYCLE ELEMENTS OF myArray { ... }
/// </summary>
public class ElementsKeywordToken : Token, IKeyWordToken
{
    public ElementsKeywordToken()
    {
    }

    public ElementsKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After ELEMENTS, expect OF keyword</summary>
    public override Type[] Expectations => [typeof(OfKeywordToken)];

    public override string KeyWord => "ELEMENTS";

    public override string ToString()
    {
        return $"ElementsKeywordToken '{RawToken?.Text}'";
    }
}
