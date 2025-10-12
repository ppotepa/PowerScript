using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Operators
{
    public class CommaSeparatorToken : Token
    {
        public CommaSeparatorToken()
        {
        }

        public CommaSeparatorToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];
    }
}