using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Core.Syntax.Tokens.Operators
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