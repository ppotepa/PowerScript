using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing 'OF' keyword - used with CHAIN for array literals.
///     Example: "CHAIN OF [1, 2, 3]"
/// </summary>
public class OfKeywordToken : Token, IKeyWordToken
{
    public OfKeywordToken()
    {
    }

    public OfKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After OF, expect opening bracket for array literal</summary>
    public override Type[] Expectations => [typeof(BracketOpen)];

    public override string KeyWord => "OF";
}
