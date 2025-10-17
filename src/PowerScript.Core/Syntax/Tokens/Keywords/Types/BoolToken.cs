using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords.Types;

/// <summary>
///     Token representing the BOOL/BOOLEAN type keyword.
///     Used for boolean variables and function return types.
/// </summary>
public class BoolToken : Token, IKeyWordToken, IBaseTypeToken
{
    public BoolToken()
    {
    }

    public BoolToken(RawToken rawToken) : base(rawToken)
    {
    }

    // After BOOL, expect identifier (variable name) or CHAIN (for arrays)
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(ChainToken)];
}
