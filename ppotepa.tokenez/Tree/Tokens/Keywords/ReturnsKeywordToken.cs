using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    /// <summary>
    ///     Token representing the 'RETURNS' keyword in alternative function syntax.
    ///     Used in: FUNCTION name RETURNS TYPE WITH params { }
    /// </summary>
    public class ReturnsKeywordToken : Token
    {
        public ReturnsKeywordToken()
        {
        }

        public ReturnsKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override string KeyWord => "RETURNS";

        public override Type[] Expectations => [];
    }
}
