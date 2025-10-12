using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Operators;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    /// <summary>
    /// Token representing the NET keyword or # shorthand.
    /// Used to access .NET framework functionality directly.
    /// Example: "NET::System.Console.WriteLine(...)" or "#Console.WriteLine(...)"
    /// </summary>
    public class NetKeywordToken : Token, IKeyWordToken
    {
        public NetKeywordToken()
        {
        }

        public NetKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After NET, expect namespace operator :: OR after #, expect identifier (class name)</summary>
        public override Type[] Expectations => [typeof(NamespaceOperatorToken), typeof(Tokens.Identifiers.IdentifierToken)];

        public override string KeyWord => "NET";
    }
}
