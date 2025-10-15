using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords.Types;

/// <summary>
///     Token representing the INT type keyword.
///     Supports optional bit-width specification: INT[8], INT[16], INT[32], INT[64]
///     Default is INT[32] if not specified.
/// </summary>
public class IntToken : Token, IKeyWordToken, IBaseTypeToken
{
    public IntToken()
    {
    }

    public IntToken(RawToken rawToken) : base(rawToken)
    {
    }

    // After INT, expect identifier (variable name), CHAIN (for arrays), or [ (for bit width)
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(ChainToken), typeof(BracketOpen)];
}