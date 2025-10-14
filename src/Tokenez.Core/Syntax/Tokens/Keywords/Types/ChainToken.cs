using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Keywords.Types;

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