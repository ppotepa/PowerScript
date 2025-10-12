using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Operators
{
    public class GreaterThanOrEqualToken : Token
    {
        public GreaterThanOrEqualToken()
        {
        }

        public GreaterThanOrEqualToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [];

        public override string ToString()
        {
            return "GreaterThanOrEqualToken(>=)";
        }
    }
}