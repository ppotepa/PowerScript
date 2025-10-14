using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Keywords.Types
{
    /// <summary>
    ///     Token representing 'PREC' - the precision/float type.
    ///     Used for floating-point numbers and decimal calculations.
    ///     Example: "VAR PREC price = 19.99" or "FUNCTION calculate()[PREC]"
    /// </summary>
    public class PrecToken : Token, IKeyWordToken, IBaseTypeToken
    {
        public PrecToken()
        {
        }

        public PrecToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ChainToken)];
    }
}