using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Operators;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    /// <summary>
    /// Token representing the NET keyword.
    /// Used to access .NET framework functionality directly.
    /// Example: "NET::System.Console.WriteLine(...)"
    /// </summary>
    public class NetKeywordToken : Token, IKeyWordToken
    {
        public NetKeywordToken()
        {
        }

        public NetKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After NET, expect namespace operator ::</summary>
        public override Type[] Expectations => [typeof(NamespaceOperatorToken)];

        public override string KeyWord => "NET";
    }
}
