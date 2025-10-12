using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords.Types
{
    public class IntToken : Token, IKeyWordToken, IBaseTypeToken
    {
        public IntToken()
        {
        }

        public IntToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ChainToken)];
    }
}