using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Scoping
{
    public class ScopeStartToken : Token
    {
        public ScopeStartToken()
        {
        }

        public ScopeStartToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [
            typeof(ReturnKeywordToken)
        ];

        public override string KeyWord => "{";
    }
}
