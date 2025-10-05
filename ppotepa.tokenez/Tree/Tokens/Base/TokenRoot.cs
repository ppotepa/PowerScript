using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Keywords;

namespace ppotepa.tokenez.Tree.Tokens.Base
{
    internal class TokenRoot : Token
    {
        public TokenRoot()
        {
        }

        public TokenRoot(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [typeof(FunctionToken)];
    }
}