using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    public class ReturnKeywordToken : Token, IKeyWordToken
    {
        public ReturnKeywordToken()
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];

        public override string Word => "RETURN";
    }
}