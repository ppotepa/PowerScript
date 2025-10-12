using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords.Types
{
    /// <summary>
    ///     Token representing 'STRING' - equivalent to 'CHAR CHAIN'.
    ///     Used for string literals and text data.
    ///     Example: "VAR STRING name = "Hello"" or "FUNCTION getName()[STRING]"
    /// </summary>
    public class StringToken : Token, IKeyWordToken, IBaseTypeToken
    {
        public StringToken()
        {
        }

        public StringToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken)];
    }
}