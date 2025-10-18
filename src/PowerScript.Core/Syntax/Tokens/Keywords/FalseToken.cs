using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the FALSE boolean literal keyword.
///     Evaluates to boolean false (0 in PowerScript).
/// </summary>
public class FalseToken : Token, IKeyWordToken
{
    public FalseToken()
    {
    }

    public FalseToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After FALSE, context-dependent (can be used in expressions, assignments, etc.)</summary>
    public override Type[] Expectations => [];
}
