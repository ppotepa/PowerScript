using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Keywords
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
