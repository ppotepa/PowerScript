using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Scoping
{
    public class ScopeEndToken : Token
    {
        public ScopeEndToken()
        {
        }

        public ScopeEndToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [];

        public override string KeyWord => "}";
    }
}
