using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords.Types
{
    /// <summary>
    /// Token representing 'CHAIN' - the collection/array type modifier.
    /// Used to create collections of other types.
    /// Example: "INT CHAIN numbers" or "CHAR CHAIN text"
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
}