
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Delimiters
{
    public class ParenthesisClosed : Token
    {
        public ParenthesisClosed()
        {
        }

        public ParenthesisClosed(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [];
    }
}