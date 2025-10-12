using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Scoping;

namespace ppotepa.tokenez.Tree.Tokens.Delimiters
{
    /// <summary>
    ///     Token representing ']' - closing square bracket.
    ///     Used to close function return type declarations.
    ///     Example: "FUNCTION add(a, b)[INT]" - the ']' after return type
    /// </summary>
    public class BracketClosed : Token
    {
        public BracketClosed()
        {
        }

        public BracketClosed(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After ']', expect scope start token '{'</summary>
        public override Type[] Expectations => [typeof(ScopeStartToken)];
    }
}