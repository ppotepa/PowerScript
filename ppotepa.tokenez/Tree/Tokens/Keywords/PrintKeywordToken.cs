using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    /// <summary>
    /// Token representing the PRINT keyword.
    /// Used to print output to console (maps to Console.WriteLine).
    /// Example: "PRINT ( "Hello World" )"
    /// </summary>
    public class PrintKeywordToken : Token, IKeyWordToken
    {
        public PrintKeywordToken()
        {
        }

        public PrintKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After PRINT, expect opening parenthesis for arguments</summary>
        public override Type[] Expectations => [typeof(ParenthesisOpen)];

        public override string KeyWord => "PRINT";
    }
}
