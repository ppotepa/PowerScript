using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords.Types;

/// <summary>
///     Token representing 'CHAR' - the character type.
///     Used for single character values.
///     Example: "VAR CHAR letter = 'A'" or "CHAR CHAIN text" (which equals STRING)
/// </summary>
public class CharToken : Token, IKeyWordToken, IBaseTypeToken
{
    public CharToken()
    {
    }

    public CharToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override Type[] Expectations => [typeof(IdentifierToken), typeof(ChainToken)];
}