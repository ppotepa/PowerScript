using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Scoping;

namespace Tokenez.Core.Syntax.Tokens.Delimiters
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