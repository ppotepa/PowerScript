using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the TRUE boolean literal keyword.
///     Evaluates to boolean true (1 in PowerScript).
/// </summary>
public class TrueToken : Token, IKeyWordToken
{
    public TrueToken()
    {
    }

    public TrueToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After TRUE, context-dependent (can be used in expressions, assignments, etc.)</summary>
    public override Type[] Expectations => [];
}
