using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Delimiters
{
    public class ParenthesisOpen : Token
    {
        public ParenthesisOpen()
        {
        }

        public ParenthesisOpen(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [];
    }
}