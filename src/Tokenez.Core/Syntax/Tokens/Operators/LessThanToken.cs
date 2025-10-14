using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Operators
{
    public class LessThanToken : Token
    {
        public LessThanToken()
        {
        }

        public LessThanToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [];

        public override string ToString()
        {
            return "LessThanToken(<)";
        }
    }
}