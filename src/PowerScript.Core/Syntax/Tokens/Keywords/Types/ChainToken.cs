using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords.Types;

/// <summary>
///     Token representing 'CHAIN' - the collection/array type modifier.
///     Used to create collections of other types.
///     Example: "INT CHAIN numbers" or "CHAR CHAIN text"
/// </summary>
public class ChainToken : Token, IKeyWordToken, ITypeToken
{
    public ChainToken()
    {
    }

    public ChainToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override Type[] Expectations => [typeof(IdentifierToken)];
}