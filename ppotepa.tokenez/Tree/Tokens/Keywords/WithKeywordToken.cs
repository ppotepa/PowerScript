using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    /// <summary>
    ///     Token representing the 'WITH' keyword in alternative function syntax.
    ///     Used in: FUNCTION name RETURNS TYPE WITH params { }
    /// </summary>
    public class WithKeywordToken : Token
    {
        public WithKeywordToken()
        {
        }

        public WithKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override string KeyWord => "WITH";

        public override Type[] Expectations => [];
    }
}
