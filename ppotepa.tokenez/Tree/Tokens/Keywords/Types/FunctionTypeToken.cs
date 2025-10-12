using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords.Types
{
    public class FunctionTypeToken : Token
    {
        public FunctionTypeToken()
        {
        }

        public FunctionTypeToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken)];
    }
}