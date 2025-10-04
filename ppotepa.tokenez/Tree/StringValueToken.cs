using ppotepa.tokenez.Tree.Tokens;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree
{
    internal class StringValueToken : Token
    {
        public StringValueToken()
        {
        }

        public StringValueToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Token[] Expects => [];
    }
}