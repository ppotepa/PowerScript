using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Delimiters
{
    /// <summary>
    /// Token representing ',' - the comma delimiter.
    /// Used to separate items in lists, function parameters, array literals, etc.
    /// </summary>
    public class CommaToken : Token
    {
        public CommaToken()
        {
        }

        public CommaToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After ',', expect another value or closing delimiter</summary>
        public override Type[] Expectations => [];

        public override string KeyWord => ",";
    }
}
