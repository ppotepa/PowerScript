using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Keywords.Types
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