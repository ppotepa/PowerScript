using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens
{
    internal class TokenRoot : Token
    {
        public TokenRoot()
        {
        }

        public TokenRoot(RawToken rawToken) : base(rawToken)
        {
        }

        public override Token[] Expects => [new FunctionToken()];
    }
}