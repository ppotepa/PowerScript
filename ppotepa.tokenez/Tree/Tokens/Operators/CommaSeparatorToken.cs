using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Operators
{
    public class CommaSeparatorToken : Token
    {
        public CommaSeparatorToken()
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];
      
    }
}