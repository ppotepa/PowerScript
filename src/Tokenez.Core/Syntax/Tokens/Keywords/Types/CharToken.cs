using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Keywords.Types
{
    /// <summary>
    ///     Token representing 'CHAR' - the character type.
    ///     Used for single character values.
    ///     Example: "VAR CHAR letter = 'A'" or "CHAR CHAIN text" (which equals STRING)
    /// </summary>
    public class CharToken : Token, IKeyWordToken, IBaseTypeToken
    {
        public CharToken()
        {
        }

        public CharToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ChainToken)];
    }
}