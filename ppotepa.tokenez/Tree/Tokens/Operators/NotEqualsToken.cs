using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Operators
{
    public class NotEqualsToken : Token
    {
        public NotEqualsToken()
        {
        }

        public NotEqualsToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [];

        public override string ToString()
        {
            return "NotEqualsToken(!=)";
        }
    }
}